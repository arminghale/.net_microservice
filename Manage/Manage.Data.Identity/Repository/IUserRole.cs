using Manage.Data.Identity.Models;
using Microsoft.EntityFrameworkCore;

namespace Manage.Data.Identity.Repository
{
    public interface IUserRole : ICommon<UserRole>
    {
        Task<UserRole?> GetByUser(int userid);
        Task<UserRole?> GetByRole(int roleid);
    }
    public class UserRoleEF : IUserRole
    {
        private readonly Context _context;
        public UserRoleEF(Context context)
        {
            _context = context;
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                UserRole? item = await _context.UserRole.FindAsync(id);
                if (item != null)
                {
                    _context.UserRole.Remove(item);
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public IQueryable<UserRole> GetAllNoTrack()
        {
            return _context.UserRole
                .AsNoTracking()
                .Include(w => w.User)
                .Include(w => w.Tenant)
                .Include(w => w.Role)
                .AsQueryable();
        }

        public async Task<UserRole?> GetByUser(int userid)
        {
            return await _context.UserRole
                .Include(w => w.User)
                .Include(w => w.Tenant)
                .Include(w => w.Role)
                .AsSplitQuery()
                .FirstOrDefaultAsync(w => w.UserId == userid);
        }
        public async Task<UserRole?> GetByRole(int roleid)
        {
            return await _context.UserRole
                .Include(w => w.User)
                .Include(w => w.Tenant)
                .Include(w => w.Role)
                .AsSplitQuery()
                .FirstOrDefaultAsync(w => w.RoleId == roleid);
        }

        public async Task<bool> MockDelete(int id)
        {
            try
            {
                UserRole? item = await _context.UserRole.FindAsync(id);
                if (item != null)
                {
                    item.Delete = true;
                    _context.Update(item);
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public async Task Save()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UnMockDelete(int id)
        {
            try
            {
                UserRole? item = await _context.UserRole.FindAsync(id);
                if (item != null)
                {
                    item.Delete = false;
                    _context.Update(item);

                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        bool ICommon<UserRole>.Delete(UserRole item)
        {
            try
            {
                _context.Remove(item);
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        IQueryable<UserRole> ICommon<UserRole>.GetAll()
        {
            return _context.UserRole
                .Include(w => w.User)
                .Include(w => w.Tenant)
                .Include(w => w.Role)

                .AsQueryable();
        }

        async Task<UserRole?> ICommon<UserRole>.GetByID(int id)
        {
            UserRole? item = await _context.UserRole
                .Include(w => w.User)
                .Include(w => w.Tenant)
                .Include(w => w.Role)
                .AsSplitQuery()
                .FirstOrDefaultAsync(w => w.Id == id);
            if (item != null)
            {
                return item;
            }
            return null;
        }

        async Task<bool> ICommon<UserRole>.Insert(UserRole item)
        {
            try
            {
                await _context.AddAsync(item);
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        bool ICommon<UserRole>.Update(UserRole item)
        {
            try
            {
                _context.Update(item);
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }
    }
}
