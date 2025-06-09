using Grpc.Core;
using Manage.Data.Identity.Repository;
using Newtonsoft.Json.Linq;

namespace Manage.gRPC.Identity.Services
{
    public class AccessService : Accesses.AccessesBase
    {
        private readonly ILogger<AccessService> _logger;
        private readonly IAccess _access;
        public AccessService(ILogger<AccessService> logger, IAccess access)
        {
            _logger = logger;
            _access = access;
        }

        public override async Task<AccessResponse> SendAccess(AccessRequest request, ServerCallContext context)
        {
            var accesses = (_access.GetByTokenAndTenant(request.Token, request.TenantId))
                .Select(w=>new AccessModel
                {
                    UserId = w.UserId,
                    ActionId = w.ActionId,
                    ActionGroupId = w.ActionGroupId,
                    ServiceId = w.ServiceId,
                    TenantId = w.TenantId,
                    Type = w.type,
                    ActionName = w.ActionName,
                    ActionURL = w.ActionURL,
                    ActionType = w.ActionType
                });
            var response = new AccessResponse();
            response.Accesses.AddRange(accesses);
            return await Task.FromResult(response);
        }
    }
}
