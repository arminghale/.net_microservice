using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Manage.Data.Reminder.Repository;
using System.Text;
using System.Text.Json;
using Manage.Data.Reminder.DTO.General;
using Asp.Versioning;
using Manage.Data.Reminder.DTO.UserSubscription;
using Manage.Data.Reminder.Models;
using Manage.Data.Public.Authorization;


namespace Manage.Reminder.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/{tenantid}/[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(BadrequestResponse), StatusCodes.Status400BadRequest)]
    public class UserSubscriptionApiController : ControllerBase
    {
        private readonly IUserSubscription _userSub;
        public UserSubscriptionApiController(IUserSubscription userSub)
        {
            _userSub = userSub;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<UserSubscriptionList>), StatusCodes.Status200OK)]
        public IActionResult Get(int tenantid, int userid = -1,string search = "")
        {
            try
            {
                var list = _userSub.GetByTenant(tenantid);
                if (!User.HasClaim("realDelete", "1"))
                {
                    list = list.Where(w => !w.Delete);
                }
                if (userid>0)
                {
                    list.Where(w => w.UserId == userid);
                }
                if (!string.IsNullOrEmpty(search))
                {
                    list = list.Where(w => w.Subscription.Title.ToLower().Contains(search.ToLower()));
                }

                return Content(JsonSerializer.Serialize(list.Select(w => new UserSubscriptionList(w)).ToList()), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserSubscriptionOne), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int tenantid, int id)
        {
            try
            {
                var userSubscription = await _userSub.GetByID(id);

                if (userSubscription == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (userSubscription.Delete && !User.HasClaim("realDelete", "1"))
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (userSubscription.Subscription.TenantId != tenantid)
                {
                    return BadRequest(new { message = "Not Found" });
                }

                return Content(JsonSerializer.Serialize(new UserSubscriptionOne(userSubscription)), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Post(int tenantid, [FromBody] PostUserSubscription postUserSubscription)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {

                var userid = int.Parse(User.Claims.FirstOrDefault(w => w.Type == "uid").Value);
                var userSubs = _userSub.GetByUser(userid).Where(w=>w.Subscription.TenantId==tenantid).ToList();
                if (userSubs.Count>0 && userSubs.LastOrDefault().Reminders.Count<userSubs.LastOrDefault().Subscription.ReminderLimit)
                {
                    return BadRequest(new { message = "A valid subscription is active" });
                }

                var userSubscription = new UserSubscription
                {
                    SubscriptionId = postUserSubscription.subscriptionId,
                    UserId = userid,
                    Status = SubscriptionStatus.PENDING
                };
                await _userSub.Insert(userSubscription);
                await _userSub.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }

        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Put(int tenantid, int id, [FromBody] PutUserSubscription putUserSubscription)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {
                if (id != putUserSubscription.id)
                {
                    return BadRequest(new { message = "Different ID" });
                }

                var userSubscription = await _userSub.GetByID(id);
                if (userSubscription == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (userSubscription.Subscription.TenantId != tenantid)
                {
                    return BadRequest(new { message = "Not Found" });
                }

                userSubscription.SubscriptionId = putUserSubscription.subscriptionId;
                userSubscription.Status = putUserSubscription.status;
                userSubscription.LastUpdateDate = DateTime.Now;
                _userSub.Update(userSubscription);
                await _userSub.Save();

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
                var userSubscription = await _userSub.GetByID(id);
                if (userSubscription == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (userSubscription.Subscription.TenantId != tenantid)
                {
                    return BadRequest(new { message = "Not Found" });
                }

                if (mock)
                {
                    await _userSub.MockDelete(userSubscription.Id);
                }
                else
                {
                    if (!User.HasClaim("realDelete", "1"))
                    {
                        return Unauthorized(new { message = "Forbidden" });
                    }
                    _userSub.Delete(userSubscription);
                }

                await _userSub.Save();

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
                var userSubscription = await _userSub.GetByID(id);
                if (userSubscription == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (userSubscription.Subscription.TenantId != tenantid)
                {
                    return BadRequest(new { message = "Not Found" });
                }

                await _userSub.UnMockDelete(userSubscription.Id);
                await _userSub.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

    }

    public record PostUserSubscription
    {
        public int subscriptionId { get; init; }
        
    }
    public record PutUserSubscription
    {
        public int id { get; init; }
        public int subscriptionId { get; init; }
        public SubscriptionStatus status { get; init; }

    }
}
