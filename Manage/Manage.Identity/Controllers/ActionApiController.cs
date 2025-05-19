using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Manage.Data.Management.Repository;
using System.Text;
using System.Text.Json;
using Manage.Data.Management.DTO.General;
using Asp.Versioning;
using Manage.Data.Management.DTO.Action;
using Action = Manage.Data.Management.Models.Action;
using Manage.Data.Public.Authorization;
using Manage.Identity.Middlewares;


namespace Manage.Identity.Controllers
{
    [TypeFilter(typeof(AuthorizationFilter))]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(BadrequestResponse), StatusCodes.Status400BadRequest)]
    public class ActionApiController : ControllerBase
    {
        private readonly IAction _action;
        public ActionApiController(IAction _action)
        {
            this._action = _action;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ActionList>), StatusCodes.Status200OK)]
        public IActionResult Get(string search = "", int actiongroupid = -1, int serviceid = -1)
        {
            try
            {
                IQueryable<Action> list = null;
                if (serviceid > 0)
                {
                    list = _action.GetByService(serviceid);
                }
                else if (actiongroupid > 0)
                {
                    list = _action.GetByActionGroup(actiongroupid);
                }
                else
                {
                    list = _action.GetAllNoTrack();
                }
                
                if (!User.HasClaim("realDelete", "1"))
                {
                    list = list.Where(w => !w.Delete);
                }


                if (!string.IsNullOrEmpty(search))
                {
                    list = list.Where(w => w.Title.ToLower().Contains(search.ToLower()));
                }

                return Content(JsonSerializer.Serialize(list.Select(w => new ActionList(w)).ToList()), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ActionList), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var action = await _action.GetByID(id);
                if (action == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (action.Delete && !User.HasClaim("realDelete", "1"))
                {
                    return BadRequest(new { message = "Not Found" });
                }

                return Content(JsonSerializer.Serialize(new ActionList(action)), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] PostAction postAction)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {
                var action = await _action.GetByTitleAndActionGroup(postAction.title,postAction.actiongroupid);
                if (action != null)
                {
                    return BadRequest(new { message = "Action with same title exists" });
                }
                action = await _action.GetByURLAndActionGroup(postAction.url, postAction.actiongroupid);
                if (action != null)
                {
                    return BadRequest(new { message = "Action with same url exists" });
                }
                if (!new string[] { "GET","POST","PUT","DELETE"}.Any(w => postAction.type==w))
                {
                    return BadRequest(new { message = "Type is not valid (GET|POST|PUT|DELETE)" });
                }

                action = new Action
                {
                    ActionGroupId = postAction.actiongroupid,
                    Title = postAction.title,
                    URL = !postAction.url.StartsWith('/') ? '/' + postAction.url : postAction.url,
                    Type = postAction.type,
                };
                await _action.Insert(action);
                await _action.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }

        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Put(int id, [FromBody] PutAction putAction)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {
                if (id != putAction.id)
                {
                    return BadRequest(new { message = "Different ID" });
                }

                var action = await _action.GetByID(id);
                if (action == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (action.Title != putAction.title && await _action.GetByTitleAndActionGroup(putAction.title, putAction.actiongroupid) != null)
                {
                    return BadRequest(new { message = "Action with same title exists" });
                }
                if (action.Title != putAction.title && await _action.GetByURLAndActionGroup(putAction.url, putAction.actiongroupid) != null)
                {
                    return BadRequest(new { message = "Action with same url exists" });
                }
                if (!new string[] { "GET", "POST", "PUT", "DELETE" }.Any(w => putAction.type == w))
                {
                    return BadRequest(new { message = "Type is not valid (GET|POST|PUT|DELETE)" });
                }


                action.Title = putAction.title;
                action.URL = !putAction.url.StartsWith('/') ? '/' + putAction.url : putAction.url;
                action.Type = putAction.type;
                action.ActionGroupId = putAction.actiongroupid;
                action.LastUpdateDate = DateTime.Now;
                _action.Update(action);
                await _action.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(int id, bool mock = true)
        {
            try
            {
                var action = await _action.GetByID(id);
                if (action == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (mock)
                {
                    await _action.MockDelete(action.Id);
                }
                else
                {
                    if (!User.HasClaim("realDelete", "1"))
                    {
                        return Unauthorized(new { message = "Forbidden" });
                    }
                    _action.Delete(action);
                }

                await _action.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpGet("unmock/{id}")]
        //[Route("api/v{version:apiVersion}/[controller]/")]
        [Authorize(Policy = "realDelete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UnMock(int id)
        {
            try
            {
                var action = await _action.GetByID(id);
                if (action == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }

                await _action.UnMockDelete(action.Id);
                await _action.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

    }

    public record PostAction
    {
        public int actiongroupid { get; set; }
        public required string title { get; init; }
        public required string url { get; init; }
        public required string type { get; init; }
    }
    public record PutAction
    {
        public int id { get; init; }
        public int actiongroupid { get; set; }
        public required string title { get; init; }
        public required string url { get; init; }
        public required string type { get; init; }

    }
}
