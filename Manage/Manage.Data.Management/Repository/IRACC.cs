using Manage.Data.Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Manage.Data.Management.Repository
{
    public interface IRACC : ICommon<RACC>
    {
        Task<RACC?> GetByRole(int roleid);
    }
    public class RACCEF : IRACC
    {
        private readonly Context _context;
        public RACCEF(Context context)
        {
            _context = context;
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                RACC? item = await _context.RACC.FindAsync(id);
                if (item != null)
                {
                    _context.RACC.Remove(item);
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public IQueryable<RACC> GetAllNoTrack()
        {
            return _context.RACC
                .AsNoTracking()
                .Include(w => w.Role)
                .Include(w => w.Tenant)
                .Include(w => w.Action)
                .AsQueryable();
        }

        public async Task<RACC?> GetByRole(int roleid)
        {
            return await _context.RACC
                .Include(w => w.Role)
                .Include(w => w.Tenant)
                .Include(w => w.Action)
                .AsSplitQuery()
                .FirstOrDefaultAsync(w => w.RoleId == roleid);
        }


        public async Task<bool> MockDelete(int id)
        {
            try
            {
                RACC? item = await _context.RACC.FindAsync(id);
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
                RACC? item = await _context.RACC.FindAsync(id);
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

        bool ICommon<RACC>.Delete(RACC item)
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

        IQueryable<RACC> ICommon<RACC>.GetAll()
        {
            return _context.RACC
                .Include(w => w.Role)
                .Include(w => w.Tenant)
                .Include(w => w.Action)

                .AsQueryable();
        }

        async Task<RACC?> ICommon<RACC>.GetByID(int id)
        {
            RACC? item = await _context.RACC
                .Include(w => w.Role)
                .Include(w => w.Tenant)
                .Include(w => w.Action)
                .AsSplitQuery()
                .FirstOrDefaultAsync(w => w.Id == id);
            if (item != null)
            {
                return item;
            }
            return null;
        }

        async Task<bool> ICommon<RACC>.Insert(RACC item)
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

        bool ICommon<RACC>.Update(RACC item)
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
