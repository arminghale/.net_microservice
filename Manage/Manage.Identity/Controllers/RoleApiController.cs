using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Manage.Data.Management.Repository;
using System.Text;
using System.Text.Json;
using Manage.Data.Management.DTO.General;
using Asp.Versioning;
using Manage.Data.Management.DTO.Role;
using Manage.Data.Management.Models;
using Manage.Data.Public.Authorization;


namespace Manage.Identity.Controllers
{
    [ClaimRequirement]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(BadrequestResponse), StatusCodes.Status400BadRequest)]
    public class RoleApiController : ControllerBase
    {
        private readonly IRole _role;
        public RoleApiController(IRole _role)
        {
            this._role = _role;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<RoleList>), StatusCodes.Status200OK)]
        public IActionResult Get(string search = "")
        {
            try
            {
                var list = _role.GetAllNoTrack();
                if (!User.HasClaim("realDelete", "1"))
                {
                    list = list.Where(w => !w.Delete);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    list = list.Where(w => w.Title.Contains(search));
                }

                return Content(JsonSerializer.Serialize(list.Select(w => new RoleList(w)).ToList()), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RoleList), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var role = await _role.GetByID(id);
                if (role == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (role.Delete && !User.HasClaim("realDelete", "1"))
                {
                    return BadRequest(new { message = "Not Found" });
                }

                return Content(JsonSerializer.Serialize(new RoleList(role)), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] PostRole postRole)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {

                var role = await _role.GetByTitle(postRole.title);
                if (role != null)
                {
                    return BadRequest(new { message = "Role with same title exists" });
                }

                role = new Role
                {
                    Title = postRole.title,
                };
                await _role.Insert(role);
                await _role.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }

        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Put(int id, [FromBody] PutRole putRole)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {
                if (id != putRole.id)
                {
                    return BadRequest(new { message = "Different ID" });
                }

                var role = await _role.GetByID(id);
                if (role == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (role.Title != putRole.title && await _role.GetByTitle(putRole.title) != null)
                {
                    return BadRequest(new { message = "Role with same title exists" });
                }

                role.Title = putRole.title;
                role.LastUpdateDate = DateTime.Now;
                _role.Update(role);
                await _role.Save();

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
                var role = await _role.GetByID(id);
                if (role == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (mock)
                {
                    await _role.MockDelete(role.Id);
                }
                else
                {
                    if (!User.HasClaim("realDelete", "1"))
                    {
                        return Unauthorized(new { message = "Forbidden" });
                    }
                    _role.Delete(role);
                }

                await _role.Save();

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
                var role = await _role.GetByID(id);
                if (role == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }

                await _role.UnMockDelete(role.Id);
                await _role.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

    }

    public record PostRole
    {
        public required string title { get; init; }
    }
    public record PutRole
    {
        public int id { get; init; }
        public required string title { get; init; }
    }
}
