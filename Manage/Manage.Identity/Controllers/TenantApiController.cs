using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Manage.Data.Identity.Repository;
using System.Text;
using System.Text.Json;
using Manage.Data.Identity.DTO.General;
using Asp.Versioning;
using Manage.Data.Identity.DTO.Tenant;
using Manage.Data.Identity.Models;
using Manage.Data.Public.Authorization;
using Manage.Identity.Middlewares;


namespace Manage.Identity.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(BadrequestResponse), StatusCodes.Status400BadRequest)]
    public class TenantApiController : ControllerBase
    {
        private readonly ITenant _tenant;
        private readonly IRole _role;
        public TenantApiController(ITenant _tenant, IRole role)
        {
            this._tenant = _tenant;
            _role = role;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<TenantList>), StatusCodes.Status200OK)]
        public IActionResult Get(string search = "")
        {
            try
            {
                var list = _tenant.GetAllNoTrack();
                if (!User.HasClaim("realDelete", "1"))
                {
                    list = list.Where(w => !w.Delete);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    list = list.Where(w => w.Title.ToLower().Contains(search.ToLower()));
                }

                return Content(JsonSerializer.Serialize(list.Select(w => new TenantList(w)).ToList()), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TenantOne), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var tenant = await _tenant.GetByID(id);
                if (tenant == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (tenant.Delete && !User.HasClaim("realDelete", "1"))
                {
                    return BadRequest(new { message = "Not Found" });
                }

                return Content(JsonSerializer.Serialize(new TenantOne(tenant)), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] PostTenant postTenant)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {

                var tenant = await _tenant.GetByTitle(postTenant.title);
                if (tenant != null)
                {
                    return BadRequest(new { message = "Tenant with same title exists" });
                }
                foreach (var item in postTenant.registerRoles.Concat(postTenant.adminRoles))
                {
                    if (await _role.GetByID(item) == null)
                    {
                        return BadRequest(new { message = "One of the roles not exists" });
                    }
                }

                tenant = new Tenant
                {
                    Title = postTenant.title,
                    Additional = postTenant.additional,
                    RegisterRoles = string.Join(",",postTenant.registerRoles),
                    AdminRoles = string.Join(",",postTenant.adminRoles),
                };
                await _tenant.Insert(tenant);
                await _tenant.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }

        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Put(int id, [FromBody] PutTenant putTenant)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {
                if (id != putTenant.id)
                {
                    return BadRequest(new { message = "Different ID" });
                }

                var tenant = await _tenant.GetByID(id);
                if (tenant == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (tenant.Title != putTenant.title && await _tenant.GetByTitle(putTenant.title) != null)
                {
                    return BadRequest(new { message = "Tenant with same title exists" });
                }
                foreach (var item in putTenant.registerRoles.Concat(putTenant.adminRoles))
                {
                    if (await _role.GetByID(item) == null)
                    {
                        return BadRequest(new { message = "One of the roles not exists" });
                    }
                }

                tenant.Title = putTenant.title;
                tenant.Additional = putTenant.additional;
                tenant.RegisterRoles = string.Join(",", putTenant.registerRoles);
                tenant.AdminRoles = string.Join(",", putTenant.adminRoles);
                tenant.LastUpdateDate = DateTime.Now;
                _tenant.Update(tenant);
                await _tenant.Save();

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
                var tenant = await _tenant.GetByID(id);
                if (tenant == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (mock)
                {
                    await _tenant.MockDelete(tenant.Id);
                }
                else
                {
                    if (!User.HasClaim("realDelete", "1"))
                    {
                        return Unauthorized(new { message = "Forbidden" });
                    }
                    _tenant.Delete(tenant);
                }

                await _tenant.Save();

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
                var tenant = await _tenant.GetByID(id);
                if (tenant == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }

                await _tenant.UnMockDelete(tenant.Id);
                await _tenant.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

    }

    public record PostTenant
    {
        public required string title { get; init; }
        public string? additional { get; init; }
        public required int[] registerRoles { get; init; }
        public required int[] adminRoles { get; init; }
        
    }
    public record PutTenant
    {
        public int id { get; init; }
        public required string title { get; init; }
        public string? additional { get; init; }
        public required int[] registerRoles { get; init; }
        public required int[] adminRoles { get; init; }

    }
}
