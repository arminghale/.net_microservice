using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Manage.Data.Public.Authorization
{
    public class ClaimRequirementFilter : IAuthorizationFilter
    {
        private int? tenant;
        public ClaimRequirementFilter()
        {
        }
        public ClaimRequirementFilter(int tenant)
        {
            this.tenant = tenant;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var hasClaim = false;
            if (tenant!=null)
            {
                hasClaim = context.HttpContext.User.Claims.Any(c => c.Type.Contains($"{context.HttpContext.Request.Method}:{tenant}{context.HttpContext.Request.Path.Value}"));
            }
            else
            {
                hasClaim = context.HttpContext.User.Claims.Any(c => c.Type.Contains($"{context.HttpContext.Request.Method}:{context.HttpContext.Request.Path.Value}"));
            }

            if (!hasClaim)
            {
                context.Result = new ForbidResult();
            }
        }
    }

    public class ClaimRequirementAttribute : TypeFilterAttribute
    {
        public ClaimRequirementAttribute() : base(typeof(ClaimRequirementFilter))
        {
        }
        public ClaimRequirementAttribute(int tenant) : base(typeof(ClaimRequirementFilter))
        {
            Arguments = new object[] {tenant };
        }
    }
}
