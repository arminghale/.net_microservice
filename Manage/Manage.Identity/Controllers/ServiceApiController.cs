using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Manage.Data.Identity.Repository;
using System.Text;
using System.Text.Json;
using Manage.Data.Identity.DTO.General;
using Asp.Versioning;
using Manage.Data.Identity.DTO.Service;
using Manage.Data.Identity.Models;
using Manage.Data.Public.Authorization;
using Manage.Identity.Middlewares;


namespace Manage.Identity.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(BadrequestResponse), StatusCodes.Status400BadRequest)]
    public class ServiceApiController : ControllerBase
    {
        private readonly IService _service;
        public ServiceApiController(IService _service)
        {
            this._service = _service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ServiceList>), StatusCodes.Status200OK)]
        public IActionResult Get(string search = "")
        {
            try
            {
                var list = _service.GetAllNoTrack();
                if (!User.HasClaim("realDelete", "1"))
                {
                    list = list.Where(w => !w.Delete);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    list = list.Where(w => w.Title.ToLower().Contains(search.ToLower()));
                }

                return Content(JsonSerializer.Serialize(list.Select(w => new ServiceList(w)).ToList()), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ServiceOne), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var service = await _service.GetByID(id);
                if (service == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (service.Delete && !User.HasClaim("realDelete", "1"))
                {
                    return BadRequest(new { message = "Not Found" });
                }

                return Content(JsonSerializer.Serialize(new ServiceOne(service)), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] PostService postService)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {

                var service = await _service.GetByTitle(postService.title);
                if (service != null)
                {
                    return BadRequest(new { message = "Service with same title exists" });
                }

                service = new Service
                {
                    Title = postService.title,
                };
                await _service.Insert(service);
                await _service.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }

        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Put(int id, [FromBody] PutService putService)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {
                if (id != putService.id)
                {
                    return BadRequest(new { message = "Different ID" });
                }

                var service = await _service.GetByID(id);
                if (service == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (service.Title != putService.title && await _service.GetByTitle(putService.title) != null)
                {
                    return BadRequest(new { message = "Service with same title exists" });
                }

                service.Title = putService.title;
                service.LastUpdateDate = DateTime.Now;
                _service.Update(service);
                await _service.Save();

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
                var service = await _service.GetByID(id);
                if (service == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }
                if (mock)
                {
                    await _service.MockDelete(service.Id);
                }
                else
                {
                    if (!User.HasClaim("realDelete", "1"))
                    {
                        return Unauthorized(new { message = "Forbidden" });
                    }
                    _service.Delete(service);
                }

                await _service.Save();

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
                var service = await _service.GetByID(id);
                if (service == null)
                {
                    return BadRequest(new { message = "Not Found" });
                }

                await _service.UnMockDelete(service.Id);
                await _service.Save();

                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

    }

    public record PostService
    {
        public required string title { get; init; }
    }
    public record PutService
    {
        public int id { get; init; }
        public required string title { get; init; }
    }
}
