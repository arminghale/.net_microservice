using Asp.Versioning;
using Manage.Data.Management.DTO.General;
using Manage.Data.Management.DTO.User;
using Manage.Data.Management.Models;
using Manage.Data.Management.Repository;
using Manage.Data.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Manage.Identity.Controllers
{
    [ApiVersion("1.0")]
    //[Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ProducesResponseType(typeof(BadrequestResponse), StatusCodes.Status400BadRequest)]
    public class AuthApiController : ControllerBase
    {
        private readonly IUser _user;
        private readonly IAccess _access;
        private readonly IUserRole _userRole;
        private readonly ITenant _tenant;
        private readonly ICache _cache;
        private readonly ISMS _sms;
        private readonly ISendMail _email;
        public AuthApiController(IUser _user, IAccess _access, IUserRole _userRole
            , ICache _cache, ISMS _sms, ISendMail _email, ITenant _tenant)
        {
            this._user = _user;
            this._userRole = _userRole;
            this._access = _access;
            this._cache = _cache;
            this._sms = _sms;
            this._email = _email;
            this._tenant = _tenant;
        }

        [HttpGet]
        [Route("api/v1/[controller]/{tenantid}/Code/{phonenumber}")]
        [ProducesResponseType(typeof(CodeResponse), StatusCodes.Status208AlreadyReported)]
        [ProducesResponseType(typeof(CodeResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Code(int tenantid, string phonenumber)
        {
            try
            {
                var tenant = await _tenant.GetByID(tenantid);
                if (tenant == null)
                {
                    return BadRequest(new { message = "Tenant not found" });
                }
                if (!(new PhoneAttribute().IsValid(phonenumber)))
                {
                    return BadRequest(new { message = "Phonenumber is invalid" });
                }

                var data = _cache.GetData<SMSCodeInfo>(phonenumber);
                if (data != null && !data.validate)
                {
                    var timeDiff = DateTime.Now - data.date;
                    if (timeDiff.Minutes < 2)
                    {
                        return StatusCode(208, new CodeResponse((int)(120 - timeDiff.TotalSeconds), "Text has been sent, try again later"));
                    }
                    _cache.RemoveData(phonenumber);
                }

                int result = await _sms.SmsCode(phonenumber, tenant.Title);

                if (result >= 2000)
                {
                    _cache.SetData<SMSCodeInfo>(phonenumber, new SMSCodeInfo(phonenumber, DateTime.Now, false), 120);
                    return Content(JsonSerializer.Serialize(new CodeResponse(120)), "application/json", Encoding.UTF8);
                }
                if (result == 8)
                {
                    return BadRequest(new { message = "SMS platform is inactive" });
                }
                return BadRequest(new { message = "Error in sending text" });
            }
            catch (Exception e)
            {

                return BadRequest(new { message = "System-Error", error = e.Message });
            }

        }

        [HttpPost]
        [Route("api/v1/[controller]/{tenantid}/Code/{phonenumber}")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CodeResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Code(int tenantid, string phonenumber, [FromBody] PostCode code)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {
                var tenant = await _tenant.GetByID(tenantid);
                if (tenant == null)
                {
                    return BadRequest(new { message = "Tenant not found" });
                }

                var data = _cache.GetData<SMSCodeInfo>(phonenumber);
                if (data == null)
                {
                    return BadRequest(new { message = "Request to send text" });
                }

                bool validateCode = await _sms.ValidateCode(code.code, phonenumber);
                if (!validateCode)
                {
                    var timeDiff = DateTime.Now - data.date;
                    return Conflict(new CodeResponse((int)(120 - timeDiff.TotalSeconds), "Wrong code"));
                }


                var user = await _user.GetByPhonenumber(phonenumber);

                if (user != null)
                {
                    user.PhonenumberValidation = "YES";
                    user.Validation = "YES";
                    user.LastLoginDate = DateTime.Now;
                    if (user.Delete)
                    {
                        user.Delete = false;
                    }
                    _user.Update(user);

                    _cache.RemoveData(phonenumber);
                }
                else
                {
                    _cache.SetData<SMSCodeInfo>(phonenumber, new SMSCodeInfo(phonenumber, DateTime.Now, true), 1800);
                    user = new User
                    {
                        Phonenumber = phonenumber,
                        PhonenumberValidation = "YES",
                        Validation = "YES",
                        Username = phonenumber,
                        Password = Hash.Hashing(phonenumber)
                    };
                    await _user.Insert(user);
                }

                await _user.Save();

                var response = new UserResponse(new UserOneItself(user), await GenerateToken(user, tenantid));

                return Content(JsonSerializer.Serialize(response), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = "System-Error", error = e.Message });
            }

        }

        [HttpPost]
        [Route("api/v1/[controller]/{tenantid}/Register")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadrequestResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Register(int tenantid, [FromBody] PostRegister register)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });

            }
            try
            {
                var tenant = await _tenant.GetByID(tenantid);
                if (tenant == null)
                {
                    return BadRequest(new { message = "Tenant not found" });
                }

                if (Request.Headers.Any(w => w.Key == HeaderNames.Authorization))
                {
                    var accessToken = Request.Headers[HeaderNames.Authorization].ToString().Split(' ')[1];
                    if (!ValidateCurrentToken(accessToken))
                    {
                        return Unauthorized(new { message = "User not Signed in or invalid token" });
                    }

                    User user = await _user.GetByID(int.Parse(User.Claims.FirstOrDefault(w => w.Type == "userid").Value));
                    if (user == null)
                    {
                        return BadRequest(new { message = "User not found" });
                    }

                    var username_user = await _user.GetByUsername(register.username);
                    if (username_user != null && username_user.Id != user.Id)
                    {
                        return BadRequest(new { message = "User with this username exists" });
                    }
                    user.Username = register.username;
                    user.LastUpdateDate = DateTime.Now;

                    if (!string.IsNullOrEmpty(register.password))
                    {
                        if (register.password != register.repassword)
                        {
                            return BadRequest(new { message = "Different password and re-password" });
                        }
                        if (register.password.Length < 8)
                        {
                            return BadRequest(new { message = "Password must be more than 8 character" });
                        }
                        user.Password = Hash.Hashing(register.password);
                    }
                    _user.Update(user);
                    await _user.Save();

                    var response = new UserResponse(new UserOneItself(user), await GenerateToken(user, tenantid));
                    return Content(JsonSerializer.Serialize(response), "application/json", Encoding.UTF8);
                }
                else
                {
                    var data = _cache.GetData<SMSCodeInfo>(register.phonenumber);
                    if (data == null)
                    {
                        return BadRequest(new { message = "Request to send text" });
                    }
                    if ((DateTime.Now - data.date).TotalMinutes > 30)
                    {
                        _cache.RemoveData(register.phonenumber);
                        return BadRequest(new { message = "Its been more than 30 minutes since your phonenumber validated, try again" });
                    }

                    User user = await _user.GetByPhonenumber(register.phonenumber);
                    if (user == null)
                    {
                        return BadRequest(new { message = "Request to send text" });
                    }

                    var username_user = await _user.GetByUsername(register.username);
                    if (username_user != null && username_user.Id != user.Id)
                    {
                        return BadRequest(new { message = "User with same username exists" });
                    }

                    var phonenumber_user = await _user.GetByPhonenumber(register.phonenumber);
                    if (phonenumber_user != null && phonenumber_user.Id != user.Id)
                    {
                        return BadRequest(new { message = "User with same phonenumber exists" });
                    }

                    if (!string.IsNullOrEmpty(register.password))
                    {
                        return BadRequest(new { message = "Fill out password" });
                    }
                    if (register.password != register.repassword)
                    {
                        return BadRequest(new { message = "Different password and re-password" });
                    }
                    if (register.password.Length < 8)
                    {
                        return BadRequest(new { message = "Password must be more than 8 character" });
                    }

                    if (!string.IsNullOrEmpty(register.refrence))
                    {
                        var parent = await _user.GetByRefrence(register.refrence);
                        if (parent != null) { 
                            user.ParentId = parent.Id; 
                        }
                    }
                    user.Username = register.username;
                    user.Password = Hash.Hashing(register.password);
                    user.PhonenumberValidation = "YES";
                    user.Validation = "YES";
                    _user.Update(user);
                    await _user.Save();

                    var roles = tenant.RegisterRoles.Split(',').Select(w => int.Parse(w));
                    foreach (var item in roles)
                    {
                        await _userRole.Insert(new Data.Management.Models.UserRole
                        {
                            RoleId = item,
                            UserId = user.Id,
                            TenantId = tenant.Id
                        });
                    }
                    await _userRole.Save();

                    var response = new UserResponse(new UserOneItself(user), await GenerateToken(user, tenantid));
                    return Content(JsonSerializer.Serialize(response), "application/json", Encoding.UTF8);
                }
            }
            catch (Exception e)
            {
                return BadRequest(new { message = "System-Error", error = e.Message }); ;
            }
        }

        [HttpPost]
        [Route("api/v1/[controller]/{tenantid}/Login")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login(int tenantid, [FromBody] PostLogin login)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });

            }
            try
            {
                var tenant = await _tenant.GetByID(tenantid);
                if (tenant == null)
                {
                    return BadRequest(new { message = "Tenant not found" });
                }

                User user = await _user.CheckLogin(login.username, login.password);
                if (user == null || user.Delete)
                {
                    return BadRequest(new { message = "Username or password is wrong" });
                }

                user.LastLoginDate = DateTime.Now;
                user.Validation = "YES";
                _user.Update(user);
                await _user.Save();

                var response = new UserResponse(new UserOneItself(user), await GenerateToken(user, tenantid));
                return Content(JsonSerializer.Serialize(response), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = "System-Error", error = e.Message }); ;
            }
        }

        [HttpPost]
        [Route("api/v1/[controller]/{tenantid}/EmailCode")]
        [Authorize]
        [ProducesResponseType(typeof(CodeResponse), StatusCodes.Status208AlreadyReported)]
        [ProducesResponseType(typeof(CodeResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> EmailCode(int tenantid, [FromBody] PostEmail email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {
                var tenant = await _tenant.GetByID(tenantid);
                if (tenant == null)
                {
                    return BadRequest(new { message = "Tenant not found" });
                }

                User user = await _user.GetByID(int.Parse(User.Claims.FirstOrDefault(w => w.Type == "userid").Value));
                if (user == null || user.Delete)
                {
                    return BadRequest(new { message = "User not found" });
                }

                User email_user = await _user.GetByEmail(email.email);
                if (email_user != null && email_user.Id != user.Id)
                {
                    return BadRequest(new { message = "User with same email exists" });
                }

                var data = _cache.GetData<EmailCodeInfo>(email.email);
                if (data != null)
                {
                    var timeDiff = DateTime.Now - data.date;
                    if (timeDiff.Minutes < 2)
                    {
                        return StatusCode(208, new CodeResponse((int)(120 - timeDiff.TotalSeconds), "Email has been sent, try again later"));
                    }
                    _cache.RemoveData(email.email);
                }

                Random generator = new Random();
                var code = generator.Next(0, 1000000).ToString("D6");
                _cache.SetData<EmailCodeInfo>(email.email, new EmailCodeInfo(email.email, DateTime.Now, code), 120);
                _email.Send(new string[] { email.email }, "Code " + tenant.Title, code);
                return Content(JsonSerializer.Serialize(new CodeResponse(120)), "application/json", Encoding.UTF8);
            }
            catch (Exception e)
            {
                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }

        [HttpPost]
        [Route("api/v1/[controller]/{tenantid}/ValidateEmail")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CodeResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ValidateEmail(int tenantid, [FromBody] PostEmailCode email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Fill out all required fields in right format" });
            }
            try
            {
                var tenant = await _tenant.GetByID(tenantid);
                if (tenant == null)
                {
                    return BadRequest(new { message = "Tenant not found" });
                }

                User user = await _user.GetByID(int.Parse(User.Claims.FirstOrDefault(w => w.Type == "userid").Value));
                if (user == null || user.Delete)
                {
                    return BadRequest(new { message = "User not found" });
                }

                var data = _cache.GetData<EmailCodeInfo>(email.email);
                if (data == null)
                {
                    return BadRequest(new { message = "Request to send email" });
                }
                if (data.code != email.code)
                {
                    var timeDiff = DateTime.Now - data.date;
                    return Conflict(new CodeResponse((int)(120 - timeDiff.TotalSeconds), "Wrong code"));
                }

                _cache.RemoveData(email.email);

                user.EmailValidation = "YES";
                user.Validation = "YES";
                user.Email = email.email;
                user.LastUpdateDate = DateTime.Now;
                _user.Update(user);
                await _user.Save();

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(new { message = "System-Error", error = e.Message });
            }
        }


        [HttpGet]
        [Route("api/v1/[controller]/{tenantid}/Refresh")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BadrequestResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh(int tenantid)
        {
            try
            {
                if (!Request.Headers.Any(w => w.Key == HeaderNames.Authorization))
                {
                    return Unauthorized(new { message = "User not signed in" });
                }
                var accessToken = Request.Headers[HeaderNames.Authorization].ToString().Split(' ')[1];
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized(new { message = "User not signed in" });
                }

                User user = await _user.GetByID(int.Parse(User.Claims.FirstOrDefault(w => w.Type == "userid").Value));

                if (user == null || user.Delete)
                {
                    return BadRequest(new { message = "User not found" });
                }

                if (!ValidateCurrentToken(accessToken))
                {
                    return Unauthorized(new { message = "Invalid token" });
                }

                if (user.Delete)
                {
                    user.Delete = false;
                }
                user.LastLoginDate = DateTime.Now;
                user.Validation = "YES";
                _user.Update(user);
                await _user.Save();

                var response = new UserResponse(new UserOneItself(user), await GenerateToken(user, tenantid));

                return Content(JsonSerializer.Serialize(response), "application/json", Encoding.UTF8);

            }
            catch (Exception e)
            {
                return BadRequest(new { message = "System-Error", error = e.Message });
            }

        }

        private bool ValidateCurrentToken(string token)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY")));

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
                    ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                    IssuerSigningKey = secretKey
                }, out SecurityToken validatedToken);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private async Task<string> GenerateToken(User user, int tenantid = -1)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_KEY")));
            var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.Name, user.Username, ClaimValueTypes.String, Environment.GetEnvironmentVariable("JWT_ISSUER")));
            //userid
            if (user.Id > 0)
            {
                claims.Add(new Claim("userid", user.Id.ToString()));

                user.LastLoginDate = System.DateTime.Now;
                _user.Update(user);
                await _user.Save();
            }

            var accesses = _access.GetByUserAndTenant(user.Id,tenantid);
            foreach (var item in accesses)
            {
                claims.Add(new Claim($"{item.Action.Type}:{tenantid}{item.Action.URL}", ""));
            }

            var tokeOptions = new JwtSecurityToken(
                  expires: DateTime.Now.AddMonths(1),
                  issuer: Environment.GetEnvironmentVariable("JWT_ISSUER"),
                  audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
                  claims: claims,
                  signingCredentials: signinCredentials
              );
            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            return tokenString;
        }
    }
    public record SMSCodeInfo(string phonenumber, DateTime date, bool validate);
    public record EmailCodeInfo(string email, DateTime date, string code);
    public record CodeResponse(int time, string? message = "");
    public record PostCode
    {
        public required string code { get; init; }
    }
    public record UserResponse(UserOneItself user, string token);
    public record PostLogin
    {
        public required string username { get; init; }
        public required string password { get; init; }
    }
    public record PostRegister
    {
        public required string phonenumber { get; init; }
        public required string username { get; init; }
        public string? password { get; init; }
        public string? repassword { get; init; }
        public string? refrence { get; init; }
    };
    public record PostEditPassword
    {
        public required string password { get; init; }
        public required string repassword { get; init; }

    };
    public record PostEmail
    {
        [EmailAddress]
        public required string email { get; init; }
    }
    public record PostEmailCode
    {
        [EmailAddress]
        public required string email { get; init; }
        public required string code { get; set; }
    }

}
