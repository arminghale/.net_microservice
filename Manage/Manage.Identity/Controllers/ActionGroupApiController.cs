using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Manage.Data.Identity.Repository;
using System.Text;
using System.Text.Json;
using Manage.Data.Identity.DTO.General;
using Asp.Versioning;
using Manage.Data.Identity.DTO.ActionGroup;
using Manage.Data.Identity.Models;
using Manage.Data.Public.Authorization;
using Manage.Identity.Middlewares;


namespace Manage.Identity.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(BadrequestResponse), StatusCodes.Status400BadRequest)]
    public class ActionGroupApiController : ControllerBase
    {
        private readonly IActionGroup _actiongroup;
        public ActionGroupApiController(IActionGroup _actiongroup)
        {
            this._actiongroup = _actiongroup;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ActionGroupList>), StatusCodes.Status200OK)]
        public IActionResult Get(string search = "", int serviceid = -1)
        {
            try
            {
                IQueryable<ActionGroup> list = null;
                if(serviceid > 0)
                {
                    list = _actiongroup.GetByService(serviceid);
                }
                else
                {
                    list = _actiongroup.GetAllNoTrack();
                }
                
                if (!User.HasClaim("realDelete", "1"))
                {
                    list = list.Where(w => !w.Delete);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    list = list.Where(w => w.Title.ToLower().Contains(search.ToLower()));
                }

                return Content(JsonSerializer.Serialize(list.Select(w => new ActionGroupList(w)).ToList()), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ActionGroupOne), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var actiongroup = await _actiongroup.GetByID(id);
                if (actiongroup == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (actiongroup.Delete && !User.HasClaim("realDelete", "1"))
                {
                    return BadRequest(new { message = "Not Found" });
                }

                return Content(JsonSerializer.Serialize(new ActionGroupOne(actiongroup)), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] PostActionGroup postActionGroup)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {
                var actiongroup = await _actiongroup.GetByTitleAndService(postActionGroup.title,postActionGroup.serviceid);
                if (actiongroup != null)
                {
                    return BadRequest(new { message = "ActionGroup with same title exists" });
                }

                actiongroup = new ActionGroup
                {
                    ServiceId = postActionGroup.serviceid,
                    Title = postActionGroup.title,
                };
                await _actiongroup.Insert(actiongroup);
                await _actiongroup.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }

        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Put(int id, [FromBody] PutActionGroup putActionGroup)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {
                if (id != putActionGroup.id)
                {
                    return BadRequest(new { message = "Different ID" });
                }

                var actiongroup = await _actiongroup.GetByID(id);
                if (actiongroup == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (actiongroup.Title != putActionGroup.title && await _actiongroup.GetByTitleAndService(putActionGroup.title, putActionGroup.serviceid) != null)
                {
                    return BadRequest(new { message = "ActionGroup with same title exists" });
                }


                actiongroup.Title = putActionGroup.title;
                actiongroup.ServiceId = putActionGroup.serviceid;
                actiongroup.LastUpdateDate = DateTime.Now;
                _actiongroup.Update(actiongroup);
                await _actiongroup.Save();

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
                var actiongroup = await _actiongroup.GetByID(id);
                if (actiongroup == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (mock)
                {
                    await _actiongroup.MockDelete(actiongroup.Id);
                }
                else
                {
                    if (!User.HasClaim("realDelete", "1"))
                    {
                        return Unauthorized(new { message = "Forbidden" });
                    }
                    _actiongroup.Delete(actiongroup);
                }

                await _actiongroup.Save();

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
        public async Task<IActionResult> UnMock(int id)
        {
            try
            {
                var actiongroup = await _actiongroup.GetByID(id);
                if (actiongroup == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }

                await _actiongroup.UnMockDelete(actiongroup.Id);
                await _actiongroup.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

    }

    public record PostActionGroup
    {
        public int serviceid { get; set; }
        public required string title { get; init; }
    }
    public record PutActionGroup
    {
        public int serviceid { get; set; }
        public int id { get; init; }
        public required string title { get; init; }
    }
}
