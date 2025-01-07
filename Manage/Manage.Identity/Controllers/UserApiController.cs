using Asp.Versioning;
using Manage.Data.Management.DTO.General;
using Manage.Data.Management.DTO.User;
using Manage.Data.Management.Models;
using Manage.Data.Management.Repository;
using Manage.Data.Public;
using Manage.Data.Public.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Manage.Identity.Controllers
{
    [ClaimRequirement]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(BadrequestResponse), StatusCodes.Status400BadRequest)]
    public class UserApiController : ControllerBase
    {
        private readonly IUser _user;
        private readonly IUserRole _userRole;
        private readonly IFile _file;
        public UserApiController(IUser _user, IUserRole _userRole, IFile _file)
        {
            this._user = _user;
            this._userRole = _userRole;
            this._file = _file;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<UserList>), StatusCodes.Status200OK)]
        public IActionResult Get(string search = "")
        {
            try
            {
                var list = _user.GetAllNoTrack();
                if (!User.HasClaim("realDelete", "1"))
                {
                    list = list.Where(w => !w.Delete);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    list = list.Where(w => w.Email.Contains(search) || w.Phonenumber.Contains(search));
                }

                return Content(JsonSerializer.Serialize(list.Select(w => new UserList(w)).ToList()), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserOneAdmin), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var user = await _user.GetByID(id);
                if (user == null)
                {
                    return BadRequest(new { message = "User Not Found" });
                }
                if (user.Delete && !User.HasClaim("realDelete", "1"))
                {
                    return BadRequest(new { message = "User Not Found" });
                }

                return Content(JsonSerializer.Serialize(new UserOneAdmin(user)), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] PostUser postUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {

                var user = await _user.GetByPhonenumber(postUser.phonenumber);
                if (user != null)
                {
                    return BadRequest(new { message = "User with same phonenumber exists" });
                }
                user = await _user.GetByUsername(postUser.username);
                if (user != null)
                {
                    return BadRequest(new { message = "User with same username exists" });
                }
                if (!string.IsNullOrEmpty(postUser.email))
                {
                    user = await _user.GetByEmail(postUser.email);
                    if (user != null)
                    {
                        return BadRequest(new { message = "User with same email exists" });
                    }
                }

                var parent = await _user.GetByID(int.Parse(User.Claims.FirstOrDefault(w => w.Type == "userid").Value));

                user = new User
                {
                    ParentId = parent.Id,
                    Phonenumber = postUser.phonenumber,
                    Username = postUser.username,
                    Password = Hash.Hashing(postUser.password),
                    Email = postUser.email,
                    EmailValidation = postUser.emailvalidation ? "YES" : "NO",
                    PhonenumberValidation = postUser.phonevalidation ? "YES" : "NO",
                    Validation = postUser.validation ? "YES" : "NO"
                };
                await _user.Insert(user);
                await _user.Save();

                if (postUser.profile != null && postUser.profile.Length > 0)
                {
                    string name = user.Id
                        + Path.GetExtension(postUser.profile.FileName.ToLowerInvariant());

                    string bucket = "Profile";

                    await _file.Upload(postUser.profile.OpenReadStream(), name, bucket);
                    user.ProfilePic = $"{bucket}/{name}";
                    _user.Update(user);
                    await _user.Save();
                }


                foreach (var item in postUser.roles)
                {
                    await _userRole.Insert(new Data.Management.Models.UserRole
                    {
                        TenantId = item.tenantid,
                        RoleId = item.roleid,
                        UserId = user.Id
                    });
                }
                await _userRole.Save();


                return Ok();
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }

        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Put(int id, [FromBody] PutUser putUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {
                if (id != putUser.id)
                {
                    return BadRequest(new { message = "Different ID" });
                }

                var user = await _user.GetByID(id);
                if (user == null)
                {
                    return BadRequest(new { message = "User Not Found" });
                }

                var tuser = await _user.GetByPhonenumber(putUser.phonenumber);
                if (tuser != null && tuser.Id != user.Id)
                {
                    return BadRequest(new { message = "User with same phonenumber exists" });
                }
                tuser = await _user.GetByUsername(putUser.username);
                if (tuser != null && tuser.Id != user.Id)
                {
                    return BadRequest(new { message = "User with same username exists" });
                }
                if (!string.IsNullOrEmpty(putUser.email))
                {
                    tuser = await _user.GetByEmail(putUser.email);
                    if (tuser != null && tuser.Id != user.Id)
                    {
                        return BadRequest(new { message = "User with same email exists" });
                    }
                }


                user.Username = putUser.username;
                user.Password = Hash.Hashing(putUser.password);
                user.Email = putUser.email;
                user.EmailValidation = putUser.emailvalidation ? "YES" : "NO";
                user.PhonenumberValidation = putUser.phonevalidation ? "YES" : "NO";
                user.Validation = putUser.validation ? "YES" : "NO";
                user.Phonenumber = putUser.phonenumber;
                user.LastUpdateDate = DateTime.Now;

                if (putUser.profile != null && putUser.profile.Length > 0)
                {
                    if (!string.IsNullOrEmpty(user.ProfilePic))
                    {
                        await _file.Delete(user.ProfilePic);
                    }
                    string name = user.Id
                        + Path.GetExtension(putUser.profile.FileName.ToLowerInvariant());

                    string bucket = "Profile";

                    await _file.Upload(putUser.profile.OpenReadStream(), name, bucket);
                    user.ProfilePic = $"{bucket}/{name}";
                }
                _user.Update(user);

                foreach (var item in user.UserRoles.Except(putUser.roles.Select(w => new Data.Management.Models.UserRole
                {
                    TenantId = w.tenantid,
                    RoleId = w.roleid,
                    UserId = user.Id
                })))
                {
                    await _userRole.Delete(item.Id);
                }

                foreach (var item in putUser.roles)
                {
                    if (!user.UserRoles.Any(w => w.TenantId == item.tenantid && w.RoleId == item.roleid))
                    {
                        await _userRole.Insert(new Data.Management.Models.UserRole
                        {
                            TenantId = item.tenantid,
                            RoleId = item.roleid,
                            UserId = user.Id
                        });
                    }

                }
                await _userRole.Save();

                await _user.Save();

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
                var user = await _user.GetByID(id);
                if (user == null)
                {
                    return BadRequest(new { message = "User Not Found" });
                }
                if (mock)
                {
                    await _user.MockDelete(user.Id);
                }
                else
                {
                    if (!User.HasClaim("realDelete", "1"))
                    {
                        return Unauthorized(new { message = "Forbidden" });
                    }
                    if (!string.IsNullOrEmpty(user.ProfilePic))
                    {
                        await _file.Delete(user.ProfilePic);
                    }
                    _user.Delete(user);
                }

                await _user.Save();

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
                var user = await _user.GetByID(id);
                if (user == null)
                {
                    return BadRequest(new { message = "User Not Found" });
                }

                await _user.UnMockDelete(user.Id);
                await _user.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }


    }

    public record UserRole
    {
        public int roleid { get; init; }
        public int tenantid { get; init; }
    }
    public record PostUser
    {
        public required string phonenumber { get; init; }
        public required string username { get; init; }
        public required string password { get; init; }
        public required string email { get; init; }
        public bool phonevalidation { get; init; }
        public bool emailvalidation { get; init; }
        public bool validation { get; init; }
        public UserRole[] roles { get; init; } = Array.Empty<UserRole>();
        public IFormFile? profile { get; init; }

    };
    public record PutUser
    {
        public int id { get; set; }
        public int role { get; set; }
        public required string phonenumber { get; init; }
        public required string username { get; init; }
        public required string password { get; init; }
        public required string email { get; init; }
        public bool phonevalidation { get; init; }
        public bool emailvalidation { get; init; }
        public bool validation { get; init; }
        public UserRole[] roles { get; init; } = Array.Empty<UserRole>();
        public IFormFile? profile { get; init; }
    };

}
