using Manage.Data.Identity.Models;
using Microsoft.EntityFrameworkCore;

namespace Manage.Data.Identity.Repository
{
    public interface IRole : ICommon<Role>
    {
        Task<Role?> GetByTitle(string title);
    }
    public class RoleEF : IRole
    {
        private readonly Context _context;
        public RoleEF(Context context)
        {
            _context = context;
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                Role? item = await _context.Role.FindAsync(id);
                if (item != null)
                {
                    _context.RACC.Where(w => w.RoleId == id).ExecuteDelete();
                    _context.UserRole.Where(w => w.RoleId == id).ExecuteDelete();
                    _context.Role.Remove(item);
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public IQueryable<Role> GetAllNoTrack()
        {
            return _context.Role
                .AsNoTracking()
                .AsQueryable();
        }

        public async Task<Role?> GetByTitle(string title)
        {
            return await _context.Role
                .AsSplitQuery()
                .FirstOrDefaultAsync(w => string.Equals(w.Title, title));
        }


        public async Task<bool> MockDelete(int id)
        {
            try
            {
                Role? item = await _context.Role.FindAsync(id);
                if (item != null)
                {
                    item.Delete = true;
                    _context.Update(item);

                    await _context.RACC
                        .Where(w => w.RoleId == id)
                        .ExecuteUpdateAsync(w => w.SetProperty(o => o.Delete, true));
                    await _context.UserRole
                        .Where(w => w.RoleId == id)
                        .ExecuteUpdateAsync(w => w.SetProperty(o => o.Delete, true));

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
                Role? item = await _context.Role.FindAsync(id);
                if (item != null)
                {
                    item.Delete = false;
                    _context.Update(item);

                    await _context.RACC
                        .Where(w => w.RoleId == id)
                        .ExecuteUpdateAsync(w => w.SetProperty(o => o.Delete, false));
                    await _context.UserRole
                        .Where(w => w.RoleId == id)
                        .ExecuteUpdateAsync(w => w.SetProperty(o => o.Delete, false));

                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        bool ICommon<Role>.Delete(Role item)
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

        IQueryable<Role> ICommon<Role>.GetAll()
        {
            return _context.Role
                .AsQueryable();
        }

        async Task<Role?> ICommon<Role>.GetByID(int id)
        {
            Role? item = await _context.Role
                .AsSplitQuery()
                .FirstOrDefaultAsync(w => w.Id == id);
            if (item != null)
            {
                return item;
            }
            return null;
        }

        async Task<bool> ICommon<Role>.Insert(Role item)
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

        bool ICommon<Role>.Update(Role item)
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
