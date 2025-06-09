using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Manage.Data.Identity.Repository;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
            if (context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>()!=null)
            {
                return true;
            }
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

            var tokenTenant = int.Parse(user.Claims.FirstOrDefault(w => w.Type == "tenant").Value);
            if(int.TryParse(context.HttpContext.Request.Path.Value.Split('/')[3], out var urlToken))
            {
                if (tokenTenant!=urlToken)
                {
                    _logger.LogError("Requested tenant and access tenant are different");
                    return false;
                }
            }


            var access = _access.GetByTokenAndTenant(token, tokenTenant);
            if (access==null)
            {
                _logger.LogError("User has no access at all");
                return false;
            }
            var requestedURL = access.FirstOrDefault(w => context.HttpContext.Request.Path.Value.ToLower().Contains(w.ActionURL.ToLower())
                && string.Equals(context.HttpContext.Request.Method, w.ActionType));
            if (requestedURL==null)
            {
                _logger.LogError("User does not has access to this URL");
                return false;
            }
            if (requestedURL.ActionType=="DELETE")
            {
                var unmockURL = string.Join('/',context.HttpContext.Request.Path.Value.Split('/').Select(w=>w.ToLower().Replace("delete","unmock")));
                
                if (access.Any(w=>unmockURL.ToLower().Contains(w.ActionURL.ToLower())))
                {
                    context.HttpContext.User.Claims.Append(new Claim("realDelete", "1"));
                }
            }
            return true;
        }
    }
}
