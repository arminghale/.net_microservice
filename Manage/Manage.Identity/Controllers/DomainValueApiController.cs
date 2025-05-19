using Asp.Versioning;
using Manage.Data.Management.DTO.DomainValue;
using Manage.Data.Management.DTO.General;
using Manage.Data.Management.Models;
using Manage.Data.Management.Repository;
using Manage.Data.Public;
using Manage.Data.Public.Authorization;
using Manage.Identity.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minio.DataModel;
using System.Text;
using System.Text.Json;

namespace Manage.Identity.Controllers
{
    [TypeFilter(typeof(AuthorizationFilter))]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(BadrequestResponse), StatusCodes.Status400BadRequest)]
    public class DomainValueApiController : ControllerBase
    {
        private readonly IDomainValue _domainValue;
        private readonly IDomain _domain;
        private readonly ISubDomainValue _subDomainValue;
        private readonly IFile _file;

        public DomainValueApiController(IDomainValue _domainValue, IDomain _domain, ISubDomainValue _subDomainValue, IFile _file)
        {
            this._domainValue = _domainValue;
            this._domain = _domain;
            this._subDomainValue = _subDomainValue;
            this._file = _file;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(List<DomainValueList>), StatusCodes.Status200OK)]
        public IActionResult Get(string search = "", int domainid = -1, int parentid = -1, bool parents = false)
        {
            try
            {
                IEnumerable<DomainValue>? list = null;

                if (parentid > 0)
                {
                    list = _domainValue.GetByParent(parentid);
                }
                else if (parents)
                {
                    list = _domainValue.GetParents();
                }
                else if (domainid > 0)
                {
                    list = _domainValue.GetByDomain(domainid);
                }
                else
                {
                    list = _domainValue.GetAllNoTrack();
                }

                if (User.HasClaim("realDelete", "1"))
                {
                    list = list.Where(w => !w.Delete);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    list = list.Where(w => w.Value.ToLower().Contains(search.ToLower()));
                }

                return Content(JsonSerializer.Serialize(list.Select(w => new DomainValueList(w)).ToList()), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DomainValue), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var domainValue = await _domainValue.GetByID(id);
                if (domainValue == null || domainValue.Delete)
                {
                    return BadRequest(new { message = "Domain Not Found" });
                }
                if (domainValue.Delete && !User.HasClaim("realDelete", "1"))
                {
                    return BadRequest(new { message = "Domain Not Found" });
                }

                return Content(JsonSerializer.Serialize(new DomainValueOne(domainValue)), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromForm] PostDomainValue postDomainValue)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {

                var domain = await _domain.GetByID(postDomainValue.domainid);
                if (domain == null)
                {
                    return BadRequest(new { message = "Domain Not Found" });
                }

                var domainValue = new DomainValue
                {
                    Value = postDomainValue.value,
                    DomainId = postDomainValue.domainid
                };

                await _domainValue.Insert(domainValue);
                await _domainValue.Save();

                if (postDomainValue.file != null && postDomainValue.file.Length > 0)
                {
                    string name = postDomainValue.value
                        + Path.GetExtension(postDomainValue.file.FileName.ToLowerInvariant());

                    string bucket = "DomainValue";

                    await _file.Upload(postDomainValue.file.OpenReadStream(),name,bucket);
                    domainValue.Value = "file:" + $"{bucket}/{name}";
                    _domainValue.Update(domainValue);
                    await _domainValue.Save();
                }

                if (postDomainValue.parentid > 0)
                {
                    var parent = await _domainValue.GetByID(postDomainValue.parentid);
                    if (domainValue == null)
                    {
                        return BadRequest(new { message = "پدر DomainValue Not Found" });
                    }
                    await _subDomainValue.AddChildToParent(postDomainValue.parentid, new int[] { domainValue.Id });
                    await _domainValue.Save();
                }

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }

        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Put(int id, [FromForm] PutDomainValue putDomainValue)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {
                if (id != putDomainValue.id)
                {
                    return BadRequest(new { message = "Different ID" });
                }

                var domainValue = await _domainValue.GetByID(id);
                if (domainValue == null)
                {
                    return BadRequest(new { message = "DomainValue Not Found" });
                }

                var domain = await _domain.GetByID(putDomainValue.domainid);
                if (domain == null)
                {
                    return BadRequest(new { message = "Domain Not Found" });
                }

                domainValue.Value = putDomainValue.value;
                domainValue.DomainId = putDomainValue.domainid;
                domainValue.LastUpdateDate = DateTime.Now;

                if (putDomainValue.file != null && putDomainValue.file.Length > 0)
                {
                    if (domainValue.Value.StartsWith("file:"))
                    {
                        await _file.Delete(domainValue.Value.Split("file:")[1]);
                    }

                    string name = putDomainValue.value
                        + Path.GetExtension(putDomainValue.file.FileName.ToLowerInvariant());

                    string bucket = "DomainValue";

                    await _file.Upload(putDomainValue.file.OpenReadStream(), name, bucket);
                    domainValue.Value = "file:" + $"{bucket}/{name}";
                }

                if (putDomainValue.parentid > 0)
                {
                    var parent = await _domainValue.GetByID(putDomainValue.parentid);
                    if (domainValue == null)
                    {
                        return BadRequest(new { message = "پدر DomainValue Not Found" });
                    }
                    await _subDomainValue.AddChildToParent(putDomainValue.parentid, new int[] { domainValue.Id });
                }

                _domainValue.Update(domainValue);
                await _domainValue.Save();

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
                var domainValue = await _domainValue.GetByID(id);
                if (domainValue == null)
                {
                    return BadRequest(new { message = "DomainValue Not Found" });
                }

                if (mock)
                {
                    await _domainValue.MockDelete(domainValue.Id);
                }
                else
                {
                    if (!User.HasClaim("realDelete", "1"))
                    {
                        return Unauthorized(new { message = "Forbidden" });
                    }
                    if (domainValue.Value.Contains("file:"))
                    {
                        await _file.Delete(domainValue.Value.Split("file:")[1]);
                    }
                    _domainValue.Delete(domainValue);
                }

                await _domainValue.Save();

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
                var domainValue = await _domainValue.GetByID(id);
                if (domainValue == null)
                {
                    return BadRequest(new { message = "DomainValue Not Found" });
                }

                await _domainValue.UnMockDelete(domainValue.Id);
                await _domainValue.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

    }

    // if using file, value consider as name of the file
    // for location value must be in this format -> location:logitude|latitude
    public record PostDomainValue
    {
        public required string value { get; init; }
        public IFormFile? file { get; set; }
        public int domainid { get; init; }
        public int parentid { get; init; }
    }
    public record PutDomainValue
    {
        public int id { get; init; }
        public int domainid { get; set; }
        public required string value { get; init; }
        public IFormFile? file { get; set; }
        public int parentid { get; init; }
    }

}
