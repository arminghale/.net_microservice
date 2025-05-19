using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Manage.Data.Management.Repository;
using System.Security.Claims;

namespace Manage.Identity.Middlewares
{
    public class AuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IAccess _access;
        private readonly ILogger<AuthorizationFilter> _logger;
        public AuthorizationFilter(IAccess access, ILogger<AuthorizationFilter> logger)
        {
            _access = access;
            _logger = logger;
        }
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            bool isAuthorized = await CheckUserAuthorizationAsync(context);
            if (!isAuthorized)
            {
                context.Result = new ForbidResult();
            }
        }
        private async Task<bool> CheckUserAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (user.Identity?.IsAuthenticated==false) 
            {
                _logger.LogError("User not Authenticated");
                return false;
            }
            if (!user.Claims.Any(w=>w.Type==ClaimTypes.Authentication))
            {
                _logger.LogError("User does not have token");
                return false;
            }

            var token = user.Claims.FirstOrDefault(w => w.Type == ClaimTypes.Authentication)?.Value;
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("User does not have personal token");
                return false;
            }

            var access = _access.GetByTokenAndTenant(token,int.Parse(user.Claims.FirstOrDefault(w=>w.Type=="tenant").Value));
            if (access==null)
            {
                _logger.LogError("User has no access at all");
                return false;
            }
            if (!access.Any(w=> context.HttpContext.Request.Path.Value.ToLower().Contains(w.ActionURL.ToLower())
                && string.Equals(context.HttpContext.Request.Method,w.ActionType)))
            {
                _logger.LogError("User does not has access to this URL");
                return false;
            }

            return true;
        }
    }
}
