using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Manage.Data.Identity.Repository;
using System.Text;
using System.Text.Json;
using Manage.Data.Identity.DTO.General;
using Asp.Versioning;
using Manage.Data.Identity.DTO.Role;
using Manage.Data.Identity.Models;
using Manage.Data.Public.Authorization;
using Manage.Identity.Middlewares;


namespace Manage.Identity.Controllers
{
    [ApiVersion("1.0")]
    //[Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(BadrequestResponse), StatusCodes.Status400BadRequest)]
    public class RACCApiController : ControllerBase
    {
        private readonly IRole _role;
        private readonly IRACC _racc;
        private readonly IAction _action;
        private readonly ITenant _tenant;
        public RACCApiController(IRole _role, IRACC _racc, IAction _action, ITenant _tenant)
        {
            this._role = _role;
            this._racc = _racc;
            this._action = _action;
            this._tenant = _tenant;
        }

        [HttpPost]
        [Route("api/v{version:apiVersion}/[controller]/update/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] IEnumerable<PostRACC> postRACC)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {
                foreach (var item in postRACC)
                {
                    var role = await _role.GetByID(item.roleid);
                    if (role == null)
                    {
                        return BadRequest(new { message = "Role not found" });
                    }
                    var action = await _action.GetByID(item.actionid);
                    if (action == null)
                    {
                        return BadRequest(new { message = "Action not found" });
                    }
                    if (item.tenantid != null)
                    {
                        var tenant = await _tenant.GetByID(item.tenantid.Value);
                        if (tenant == null)
                        {
                            return BadRequest(new { message = "Tenant not found" });
                        }

                    }
                    if (item.type<0||item.type>1)
                    {
                        return BadRequest(new { message = "Type should be 0 for not access and 1 for access" });
                    }
                    var racc = new RACC
                    {
                        ActionId = item.actionid,
                        RoleId = item.roleid,
                        TenantId = item.tenantid,
                        type = item.type,
                    };
                    await _racc.Insert(racc);
                }

                await _racc.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }

        }

    }

    public record PostRACC
    {
        public int roleid { get; init; }
        public int actionid { get; init; }
        public int? tenantid { get; init; }
        public int type { get; init; }
    }
}
