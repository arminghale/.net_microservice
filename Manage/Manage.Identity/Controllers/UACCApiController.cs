using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Manage.Data.Management.Repository;
using Manage.Data.Management.DTO.General;
using Asp.Versioning;
using Manage.Data.Management.Models;
using Manage.Data.Public.Authorization;


namespace Manage.Identity.Controllers
{
    [ClaimRequirement]
    [ApiVersion("1.0")]
    //[Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(BadrequestResponse), StatusCodes.Status400BadRequest)]
    public class UACCApiController : ControllerBase
    {
        private readonly IUser _user;
        private readonly IUACC _uacc;
        private readonly IAction _action;
        private readonly ITenant _tenant;
        public UACCApiController(IUser _user, IUACC _uacc, IAction _action, ITenant _tenant)
        {
            this._user = _user;
            this._uacc = _uacc;
            this._action = _action;
            this._tenant = _tenant;
        }

        [HttpPost]
        [Route("api/v{version:apiVersion}/[controller]/update/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] IEnumerable<PostUACC> postUACC)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {
                foreach (var item in postUACC)
                {
                    var user = await _user.GetByID(item.userid);
                    if (user == null)
                    {
                        return BadRequest(new { message = "User not found" });
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
                    var uacc = new UACC
                    {
                        ActionId = item.actionid,
                        UserId = item.userid,
                        TenantId = item.tenantid,
                        type = item.type,
                    };
                    await _uacc.Insert(uacc);
                }

                await _uacc.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }

        }

    }

    public record PostUACC
    {
        public int userid { get; init; }
        public int actionid { get; init; }
        public int? tenantid { get; init; }
        public int type { get; init; }
    }
}
