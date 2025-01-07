using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Manage.Data.Management.Repository;
using System.Text;
using System.Text.Json;
using Manage.Data.Management.DTO.General;
using Asp.Versioning;
using Manage.Data.Management.DTO.Domain;
using Manage.Data.Management.Models;
using Manage.Data.Public.Authorization;


namespace Manage.Identity.Controllers
{
    [ClaimRequirement]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(BadrequestResponse), StatusCodes.Status400BadRequest)]
    public class DomainApiController : ControllerBase
    {
        private readonly IDomain _domain;
        public DomainApiController(IDomain _domain)
        {
            this._domain = _domain;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(List<DomainList>), StatusCodes.Status200OK)]
        public IActionResult Get(string search = "")
        {
            try
            {
                var list = _domain.GetAllNoTrack();
                if (!User.HasClaim("realDelete", "1"))
                {
                    list = list.Where(w => !w.Delete);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    list = list.Where(w => w.Title.Contains(search));
                }

                return Content(JsonSerializer.Serialize(list.Select(w => new DomainList(w)).ToList()), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DomainOne), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var domain = await _domain.GetByID(id);
                if (domain == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (domain.Delete && !User.HasClaim("realDelete", "1"))
                {
                    return BadRequest(new { message = "Not Found" });
                }

                return Content(JsonSerializer.Serialize(new DomainOne(domain)), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] PostDomain postDomain)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {

                var domain = await _domain.GetByTitle(postDomain.title);
                if (domain != null)
                {
                    return BadRequest(new { message = "Domain with same title exists" });
                }

                domain = new Domain
                {
                    Title = postDomain.title,
                };
                await _domain.Insert(domain);
                await _domain.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }

        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Put(int id, [FromBody] PutDomain putDomain)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {
                if (id != putDomain.id)
                {
                    return BadRequest(new { message = "Different ID" });
                }

                var domain = await _domain.GetByID(id);
                if (domain == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (domain.Title != putDomain.title && await _domain.GetByTitle(putDomain.title) != null)
                {
                    return BadRequest(new { message = "Domain with same title exists" });
                }

                domain.Title = putDomain.title;
                domain.LastUpdateDate = DateTime.Now;
                _domain.Update(domain);
                await _domain.Save();

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
                var domain = await _domain.GetByID(id);
                if (domain == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (mock)
                {
                    await _domain.MockDelete(domain.Id);
                }
                else
                {
                    if (!User.HasClaim("realDelete", "1"))
                    {
                        return Unauthorized(new { message = "Forbidden" });
                    }
                    _domain.Delete(domain);
                }

                await _domain.Save();

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
                var domain = await _domain.GetByID(id);
                if (domain == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }

                await _domain.UnMockDelete(domain.Id);
                await _domain.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

    }

    public record PostDomain
    {
        public required string title { get; init; }
    }
    public record PutDomain
    {
        public int id { get; init; }
        public required string title { get; init; }
    }
}
