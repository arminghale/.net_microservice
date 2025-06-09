using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Ocelot.Configuration;
using Ocelot.Middleware;
using Ocelot.Metadata;
using Grpc.Net.Client;
using Manage.Gateway;
using Manage.Gateway.Services;

namespace Manage.Identity.Middlewares
{
    public interface IAuthorizationService
    {
        Task<bool> CheckUserAuthorizationAsync(HttpContext context);
    }

    public class AuthorizationService : IAuthorizationService
    {
        private readonly ILogger<AuthorizationService> _logger;
        private readonly IServiceProvider _provider;
        private readonly string _gRPCAccess;
        private readonly int _cacheExpire;


        public AuthorizationService(ILogger<AuthorizationService> logger, IServiceProvider provider)
        {
            _provider = provider;
            _logger = logger;
            _gRPCAccess = Environment.GetEnvironmentVariable("gRPCAccess");
            _cacheExpire = int.Parse(Environment.GetEnvironmentVariable("AccessCacheExpirationMin"));
        }

        public async Task<bool> CheckUserAuthorizationAsync(HttpContext context)
        {
            using (var scope = _provider.CreateScope())
            {
                if (context.Items.DownstreamRoute().GetMetadata<string>("public", "") == "true")
                {
                    return true;
                }
                var _cache = scope.ServiceProvider.GetService<ICache>();
                var user = context.User;
                if (user.Identity?.IsAuthenticated == false)
                {
                    _logger.LogError("User not Authenticated");
                    return false;
                }
                if (!user.Claims.Any(w => w.Type == ClaimTypes.Authentication))
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
                if (int.TryParse(context.Request.Path.Value.Split('/')[3], out var urlToken))
                {
                    if (tokenTenant != urlToken)
                    {
                        _logger.LogError("Requested tenant and access tenant are different");
                        return false;
                    }
                }


                var access = _cache.GetData<IEnumerable<AccessModel>>(token, _cacheExpire);
                if (access == null)
                {
                    access = await GetUserAccess(token, tokenTenant);
                }

                if (access == null)
                {
                    _logger.LogError("User has no access at all");
                    return false;
                }

                _cache.SetData(token, access, _cacheExpire);

                var requestedURL = access.FirstOrDefault(w => context.Request.Path.Value.ToLower().Contains(w.ActionURL.ToLower())
                    && string.Equals(context.Request.Method, w.ActionType));
                if (requestedURL == null)
                {
                    _logger.LogError("User does not has access to this URL");
                    return false;
                }
                if (requestedURL.ActionType == "DELETE")
                {
                    var unmockURL = string.Join('/', context.Request.Path.Value.Split('/').Select(w => w.ToLower().Replace("delete", "unmock")));

                    if (access.Any(w => unmockURL.ToLower().Contains(w.ActionURL.ToLower())))
                    {
                        context.User.Claims.Append(new Claim("realDelete", "1"));
                    }
                }
                return true;
            }

        }

        private async Task<IEnumerable<AccessModel>> GetUserAccess(string token, int tenantId)
        {
            using var channel = GrpcChannel.ForAddress(_gRPCAccess);
            var client = new Accesses.AccessesClient(channel);
            var reply = await client.SendAccessAsync(
                new AccessRequest { Token = token, TenantId = tenantId });
            if (reply != null && reply.Accesses.Count > 0)
            {
                return reply.Accesses;
            }
            return null;

        }

    }
}
