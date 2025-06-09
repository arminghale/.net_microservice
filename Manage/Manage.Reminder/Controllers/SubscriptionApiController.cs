using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Manage.Data.Reminder.Repository;
using System.Text;
using System.Text.Json;
using Manage.Data.Reminder.DTO.General;
using Asp.Versioning;
using Manage.Data.Reminder.DTO.Subscription;
using Manage.Data.Reminder.Models;
using Manage.Data.Public.Authorization;


namespace Manage.Reminder.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/{tenantid}/[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(BadrequestResponse), StatusCodes.Status400BadRequest)]
    public class SubscriptionApiController : ControllerBase
    {
        private readonly ISubscription _sub;
        public SubscriptionApiController(ISubscription sub)
        {
            _sub = sub;
        }

        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<SubscriptionList>), StatusCodes.Status200OK)]
        public IActionResult Get(int tenantid,string search = "")
        {
            try
            {
                var list = _sub.GetByTenant(tenantid);
                if (!User.HasClaim("realDelete", "1"))
                {
                    list = list.Where(w => !w.Delete);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    list = list.Where(w => w.Title.ToLower().Contains(search.ToLower()));
                }

                return Content(JsonSerializer.Serialize(list.Select(w => new SubscriptionList(w)).ToList()), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(SubscriptionOne), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int tenantid, int id)
        {
            try
            {
                var subscription = await _sub.GetByID(id);

                if (subscription == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (subscription.Delete && !User.HasClaim("realDelete", "1"))
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (subscription.TenantId != tenantid)
                {
                    return BadRequest(new { message = "Not Found" });
                }

                return Content(JsonSerializer.Serialize(new SubscriptionOne(subscription)), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Post(int tenantid, [FromBody] PostSubscription postSubscription)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {

                var subscription = await _sub.GetByTitle(postSubscription.title);
                if (subscription != null && subscription.TenantId==tenantid)
                {
                    return BadRequest(new { message = "Subscription with same title exists" });
                }
                subscription = new Subscription
                {
                    Title = postSubscription.title,
                    ReminderLimit = postSubscription.limit,
                    TenantId = tenantid
                };
                await _sub.Insert(subscription);
                await _sub.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }

        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Put(int tenantid, int id, [FromBody] PutSubscription putSubscription)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {
                if (id != putSubscription.id)
                {
                    return BadRequest(new { message = "Different ID" });
                }

                var subscription = await _sub.GetByID(id);
                if (subscription == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (subscription.TenantId != tenantid)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (subscription.Title != putSubscription.title && await _sub.GetByTitle(putSubscription.title) != null)
                {

                    return BadRequest(new { message = "Subscription with same title exists" });
                }

                subscription.Title = putSubscription.title;
                subscription.ReminderLimit = putSubscription.limit;
                subscription.LastUpdateDate = DateTime.Now;
                _sub.Update(subscription);
                await _sub.Save();

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
                var subscription = await _sub.GetByID(id);
                if (subscription == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (subscription.TenantId != tenantid)
                {
                    return BadRequest(new { message = "Not Found" });
                }

                if (mock)
                {
                    await _sub.MockDelete(subscription.Id);
                }
                else
                {
                    if (!User.HasClaim("realDelete", "1"))
                    {
                        return Unauthorized(new { message = "Forbidden" });
                    }
                    _sub.Delete(subscription);
                }

                await _sub.Save();

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
                var subscription = await _sub.GetByID(id);
                if (subscription == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (subscription.TenantId != tenantid)
                {
                    return BadRequest(new { message = "Not Found" });
                }

                await _sub.UnMockDelete(subscription.Id);
                await _sub.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

    }

    public record PostSubscription
    {
        public required string title { get; init; }
        public int limit { get; init; }
        
    }
    public record PutSubscription
    {
        public int id { get; init; }
        public required string title { get; init; }
        public int limit { get; init; }

    }
}
