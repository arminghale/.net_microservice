using Microsoft.EntityFrameworkCore;
using Manage.Data.Management.Models;
using System.Threading.Tasks;
using Minio.DataModel;

namespace Manage.Data.Management.Repository
{
    public interface IAccess 
    {
        IQueryable<Access> GetByUser(int userid);
        Task<IEnumerable<Access>> GetByUserAndTenant(int userid,int tenantid);
        IEnumerable<Access> GetByTokenAndTenant(string token,int tenantid);
    }
    public class AccessEF : IAccess
    {
        private readonly Context _context;
        private readonly ICache _cache;
        private readonly int expirationMin = int.Parse(Environment.GetEnvironmentVariable("AccessCacheExpirationMin"));
        public AccessEF(Context context, ICache cache)
        {
            _context = context;
            _cache = cache;
        }

        public IEnumerable<Access> GetByTokenAndTenant(string token, int tenantid)
        {
            try
            {
                IEnumerable<Access>? userAccess = _cache.GetData<IEnumerable<Access>>($"ACCESS_{token}", expirationMin);
                if (userAccess!=null)
                {
                    return userAccess;
                }
                var userRACC = _context.RACC
                    .AsNoTracking()
                    .Include(w => w.Role)
                        .ThenInclude(r => r.UserRoles)
                            .ThenInclude(ur => ur.User)
                    .Include(w => w.Action)
                        .ThenInclude(a => a.ActionGroup)
                    .Where(w => w.TenantId == tenantid)
                    .AsSplitQuery()
                    .AsEnumerable();
                var userRACCFilter = userRACC.Where(w => w.Role.UserRoles != null && w.Role.UserRoles.Any(e => e.User.Token.Equals(token)))
                    .Select(w => new Access
                    {
                        UserId = w.Role.UserRoles.FirstOrDefault().UserId,
                        ActionGroupId = w.Action.ActionGroupId,
                        ActionId = w.ActionId,
                        ServiceId = w.Action.ActionGroup.ServiceId,
                        TenantId = w.TenantId != null ? w.TenantId.Value : -1,
                        type = w.type,
                        ActionName = w.Action.Title,
                        ActionType = w.Action.Type,
                        ActionURL = w.Action.URL
                    });
                var raccFalse = userRACCFilter.Where(w => w.type == 0);
                var raccTrue = userRACCFilter.Where(w => w.type == 1);

                var userUACC = _context.UACC
                    .AsNoTracking()
                    .Include(w => w.User)
                    .Include(w => w.Action)
                        .ThenInclude(a => a.ActionGroup)
                    .Where(w => w.User.Token.Equals(token) && w.TenantId == tenantid)
                    .Select(w => new Access
                    {
                        UserId = w.UserId,
                        ActionGroupId = w.Action.ActionGroupId,
                        ActionId = w.ActionId,
                        ServiceId = w.Action.ActionGroup.ServiceId,
                        TenantId = w.TenantId != null ? w.TenantId.Value : -1,
                        type = w.type,
                        ActionName = w.Action.Title,
                        ActionType = w.Action.Type,
                        ActionURL = w.Action.URL
                    })
                    .AsSplitQuery()
                    .AsEnumerable();
                var uaccFalse = userUACC.Where(w => w.type == 0);
                var uaccTrue = userUACC.Where(w => w.type == 1);

                var uaccAccess = uaccTrue.Where(w => !uaccFalse.Any(e => e == w));

                userAccess = raccTrue.Where(w => !raccFalse.Any(e => e == w) && !uaccFalse.Any(e => e == w)).Concat(uaccAccess);

                _cache.SetData<IEnumerable<Access>>($"ACCESS_{token}", userAccess, expirationMin);

                return userAccess;
            }
            catch (Exception)
            {

                return null;
            }
        }

        public IQueryable<Access> GetByUser(int userid)
        {
            try
            {
                var userRACC = _context.RACC
                    .AsNoTracking()
                    .Include(w => w.Role)
                        .ThenInclude(r => r.UserRoles)
                    .Include(w => w.Action)
                        .ThenInclude(a => a.ActionGroup)
                    .Where(w => w.Role.UserRoles != null && w.Role.UserRoles.Any(e => e.UserId == userid))
                    .Select(w => new Access
                    {
                        UserId = userid,
                        ActionGroupId = w.Action.ActionGroupId,
                        ActionId = w.ActionId,
                        ServiceId = w.Action.ActionGroup.ServiceId,
                        TenantId = w.TenantId!= null ? w.TenantId.Value : -1,
                        type = w.type,
                        ActionName = w.Action.Title,
                        ActionType = w.Action.Type,
                        ActionURL = w.Action.URL
                    })
                    .AsSplitQuery();
                var raccFalse = userRACC.Where(w => w.type == 0);
                var raccTrue = userRACC.Where(w => w.type == 1);

                var userUACC = _context.UACC
                    .AsNoTracking()
                    .Include(w => w.User)
                    .Include(w => w.Action)
                        .ThenInclude(a => a.ActionGroup)
                    .Where(w => w.UserId == userid)
                    .Select(w => new Access
                    {
                        UserId = userid,
                        ActionGroupId = w.Action.ActionGroupId,
                        ActionId = w.ActionId,
                        ServiceId = w.Action.ActionGroup.ServiceId,
                        TenantId = w.TenantId != null ? w.TenantId.Value : -1,
                        type = w.type,
                        ActionName = w.Action.Title,
                        ActionType = w.Action.Type,
                        ActionURL = w.Action.URL
                    })
                    .AsSplitQuery();
                var uaccFalse = userUACC.Where(w => w.type == 0);
                var uaccTrue = userUACC.Where(w => w.type == 1);

                var uaccAccess = uaccTrue.Where(w => !uaccFalse.Any(e => e == w));

                var userAcess = raccTrue.Where(w => !raccFalse.Any(e => e == w) && !uaccFalse.Any(e => e == w)).Concat(uaccAccess);
                
                return userAcess;
            }
            catch (Exception)
            {

                return null;
            }
        }

        public async Task<IEnumerable<Access>> GetByUserAndTenant(int userid, int tenantid)
        {
            try
            {
                var userRACC = _context.RACC
                    .AsNoTracking()
                    .Include(w => w.Role)
                        .ThenInclude(r => r.UserRoles)
                    .Include(w => w.Action)
                        .ThenInclude(a => a.ActionGroup)
                    .Where(w => w.TenantId == tenantid)
                    .AsSplitQuery()
                    .AsEnumerable();
                var userRACCFilter = userRACC.Where(w => w.Role.UserRoles != null && w.Role.UserRoles.Any(e => e.UserId == userid))
                    .Select(w => new Access
                    {
                        UserId = userid,
                        ActionGroupId = w.Action.ActionGroupId,
                        ActionId = w.ActionId,
                        ServiceId = w.Action.ActionGroup.ServiceId,
                        TenantId = w.TenantId != null ? w.TenantId.Value : -1,
                        type = w.type,
                        ActionName = w.Action.Title,
                        ActionType = w.Action.Type,
                        ActionURL = w.Action.URL
                    });
                var raccFalse = userRACCFilter.Where(w => w.type == 0);
                var raccTrue = userRACCFilter.Where(w => w.type == 1);

                var userUACC = _context.UACC
                    .AsNoTracking()
                    .Include(w => w.User)
                    .Include(w => w.Action)
                        .ThenInclude(a => a.ActionGroup)
                    .Where(w => w.UserId == userid && w.TenantId == tenantid)
                    .Select(w => new Access
                    {
                        UserId = userid,
                        ActionGroupId = w.Action.ActionGroupId,
                        ActionId = w.ActionId,
                        ServiceId = w.Action.ActionGroup.ServiceId,
                        TenantId = w.TenantId != null ? w.TenantId.Value : -1,
                        type = w.type,
                        ActionName = w.Action.Title,
                        ActionType = w.Action.Type,
                        ActionURL = w.Action.URL
                    })
                    .AsSplitQuery();
                var uaccFalse = userUACC.Where(w => w.type == 0);
                var uaccTrue = userUACC.Where(w => w.type == 1);

                var uaccAccess = uaccTrue.Where(w => !uaccFalse.Any(e => e == w));

                var userAcess = raccTrue.Where(w => !raccFalse.Any(e => e == w) && !uaccFalse.Any(e => e == w)).Concat(uaccAccess);

                return userAcess;
            }
            catch (Exception)
            {

                return null;
            }
        }
    }
}
