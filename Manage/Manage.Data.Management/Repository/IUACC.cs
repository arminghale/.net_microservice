using Manage.Data.Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Manage.Data.Management.Repository
{
    public interface IUACC : ICommon<UACC>
    {
        Task<UACC?> GetByUser(int userid);
    }
    public class UACCEF : IUACC
    {
        private readonly Context _context;
        public UACCEF(Context context)
        {
            _context = context;
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                UACC? item = await _context.UACC.FindAsync(id);
                if (item != null)
                {
                    _context.UACC.Remove(item);
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public IQueryable<UACC> GetAllNoTrack()
        {
            return _context.UACC
                .AsNoTracking()
                .Include(w => w.User)
                .Include(w => w.Tenant)
                .Include(w => w.Action)
                .AsQueryable();
        }

        public async Task<UACC?> GetByUser(int userid)
        {
            return await _context.UACC
                .Include(w => w.User)
                .Include(w => w.Tenant)
                .Include(w => w.Action)
                .AsSplitQuery()
                .FirstOrDefaultAsync(w => w.UserId == userid);
        }


        public async Task<bool> MockDelete(int id)
        {
            try
            {
                UACC? item = await _context.UACC.FindAsync(id);
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
                UACC? item = await _context.UACC.FindAsync(id);
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

        bool ICommon<UACC>.Delete(UACC item)
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

        IQueryable<UACC> ICommon<UACC>.GetAll()
        {
            return _context.UACC
                .Include(w => w.User)
                .Include(w => w.Tenant)
                .Include(w => w.Action)

                .AsQueryable();
        }

        async Task<UACC?> ICommon<UACC>.GetByID(int id)
        {
            UACC? item = await _context.UACC
                .Include(w => w.User)
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

        async Task<bool> ICommon<UACC>.Insert(UACC item)
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

        bool ICommon<UACC>.Update(UACC item)
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
