using Microsoft.EntityFrameworkCore;
using Manage.Data.Management.Models;

namespace Manage.Data.Management.Repository
{
    public interface IAccess 
    {
        IQueryable<Access> GetByUser(int userid);
        IQueryable<Access> GetByUserAndTenant(int userid,int tenantid);
    }
    public class AccessEF : IAccess
    {
        private readonly Context _context;
        public AccessEF(Context context)
        {
            _context = context;
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
                        Action = w.Action
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
                        Action = w.Action
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

        public IQueryable<Access> GetByUserAndTenant(int userid, int tenantid)
        {
            try
            {
                var userRACC = _context.RACC
                    .AsNoTracking()
                    .Include(w => w.Role)
                        .ThenInclude(r => r.UserRoles)
                    .Include(w => w.Action)
                        .ThenInclude(a => a.ActionGroup)
                    .Where(w => w.Role.UserRoles != null && w.Role.UserRoles.Any(e => e.UserId == userid) && w.TenantId == tenantid)
                    .Select(w => new Access
                    {
                        UserId = userid,
                        ActionGroupId = w.Action.ActionGroupId,
                        ActionId = w.ActionId,
                        ServiceId = w.Action.ActionGroup.ServiceId,
                        TenantId = w.TenantId != null ? w.TenantId.Value : -1,
                        type = w.type,
                        Action = w.Action
                    })
                    .AsSplitQuery();
                var raccFalse = userRACC.Where(w => w.type == 0);
                var raccTrue = userRACC.Where(w => w.type == 1);

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
                        Action = w.Action
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
