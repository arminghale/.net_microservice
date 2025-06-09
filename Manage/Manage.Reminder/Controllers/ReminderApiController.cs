using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Manage.Data.Reminder.Repository;
using System.Text;
using System.Text.Json;
using Manage.Data.Reminder.DTO.General;
using Asp.Versioning;
using Manage.Data.Reminder.DTO.Reminder;
using Manage.Data.Reminder.Models;
using Manage.Data.Public.Authorization;


namespace Manage.Reminder.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/{tenantid}/[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(BadrequestResponse), StatusCodes.Status400BadRequest)]
    public class ReminderApiController : ControllerBase
    {
        private readonly IReminder _reminder;
        private readonly IUserSubscription _userSub;
        public ReminderApiController(IReminder reminder, IUserSubscription userSub)
        {
            _reminder = reminder;
            _userSub = userSub;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ReminderList>), StatusCodes.Status200OK)]
        public IActionResult Get(int tenantid, int userid = -1,string search = "")
        {
            try
            {
                var list = _reminder.GetByTenant(tenantid);
                if (!User.HasClaim("realDelete", "1"))
                {
                    list = list.Where(w => !w.Delete);
                }
                if (userid>0)
                {
                    list.Where(w => w.UserSubscription.UserId == userid);
                }
                if (!string.IsNullOrEmpty(search))
                {
                    list = list.Where(w => w.Title.ToLower().Contains(search.ToLower())|| w.Description.ToLower().Contains(search.ToLower()));
                }

                return Content(JsonSerializer.Serialize(list.Select(w => new ReminderList(w)).ToList()), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ReminderOne), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int tenantid, int id)
        {
            try
            {
                var reminder = await _reminder.GetByID(id);

                if (reminder == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (reminder.Delete && !User.HasClaim("realDelete", "1"))
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (reminder.UserSubscription.Subscription.TenantId != tenantid)
                {
                    return BadRequest(new { message = "Not Found" });
                }

                return Content(JsonSerializer.Serialize(new ReminderOne(reminder)), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Post(int tenantid, [FromBody] PostReminder postReminder)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {

                var userid = int.Parse(User.Claims.FirstOrDefault(w => w.Type == "uid").Value);
                var userSubs = _userSub.GetByUser(userid).Where(w=>w.Subscription.TenantId==tenantid).ToList();
                if (userSubs.Count==0 || userSubs.LastOrDefault().Reminders.Count==userSubs.LastOrDefault().Subscription.ReminderLimit)
                {
                    return BadRequest(new { message = "There is no valid subscription" });
                }

                var reminder = new Data.Reminder.Models.Reminder
                {
                    UserSubscriptionId = userSubs.LastOrDefault().Id,
                    Title = postReminder.title,
                    Description = postReminder.description,
                    Date = DateTime.Parse(postReminder.date),
                    Status = postReminder.status
                };
                await _reminder.Insert(reminder);
                await _reminder.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }

        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Put(int tenantid, int id, [FromBody] PutReminder putReminder)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {
                if (id != putReminder.id)
                {
                    return BadRequest(new { message = "Different ID" });
                }

                var reminder = await _reminder.GetByID(id);
                if (reminder == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (reminder.UserSubscription.Subscription.TenantId != tenantid)
                {
                    return BadRequest(new { message = "Not Found" });
                }

                reminder.Title = putReminder.title;
                reminder.Description = putReminder.description;
                reminder.Status = putReminder.status;
                reminder.Date = DateTime.Parse(putReminder.date);
                reminder.LastUpdateDate = DateTime.Now;
                _reminder.Update(reminder);
                await _reminder.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(int tenantid, int id, bool mock = true)
        {
            try
            {
                var reminder = await _reminder.GetByID(id);
                if (reminder == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (reminder.UserSubscription.Subscription.TenantId != tenantid)
                {
                    return BadRequest(new { message = "Not Found" });
                }

                if (mock)
                {
                    await _reminder.MockDelete(reminder.Id);
                }
                else
                {
                    if (!User.HasClaim("realDelete", "1"))
                    {
                        return Unauthorized(new { message = "Forbidden" });
                    }
                    _reminder.Delete(reminder);
                }

                await _reminder.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpGet("unmock/{id}")]
        //[Route("api/v{version:apiVersion}/[controller]/unmock/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UnMock(int tenantid, int id)
        {
            try
            {
                var reminder = await _reminder.GetByID(id);
                if (reminder == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (reminder.UserSubscription.Subscription.TenantId != tenantid)
                {
                    return BadRequest(new { message = "Not Found" });
                }

                await _reminder.UnMockDelete(reminder.Id);
                await _reminder.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

    }

    public record PostReminder
    {
        public required string title { get; init; }
        public string description { get; init; }
        public required string date { get; init; }
        public ReminderStatus status { get; init; }

        
    }
    public record PutReminder
    {
        public int id { get; init; }
        public required string title { get; init; }
        public string description { get; init; }
        public required string date { get; init; }
        public ReminderStatus status { get; init; }

    }
}
