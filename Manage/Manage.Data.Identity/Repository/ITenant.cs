using Manage.Data.Identity.Models;
using Microsoft.EntityFrameworkCore;

namespace Manage.Data.Identity.Repository
{
    public interface ITenant : ICommon<Tenant>
    {
        Task<Tenant?> GetByTitle(string title);
    }
    public class TenantEF : ITenant
    {
        private readonly Context _context;
        public TenantEF(Context context)
        {
            _context = context;
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                Tenant? item = await _context.Tenant.FindAsync(id);
                if (item != null)
                {
                    _context.RACC.Where(w => w.TenantId == id).ExecuteDelete();
                    _context.UACC.Where(w => w.TenantId == id).ExecuteDelete();
                    _context.Tenant.Remove(item);
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public IQueryable<Tenant> GetAllNoTrack()
        {
            return _context.Tenant
                .AsNoTracking()
                .AsQueryable();
        }

        public async Task<Tenant?> GetByTitle(string title)
        {
            return await _context.Tenant
                .FirstOrDefaultAsync(w => string.Equals(w.Title, title));
        }

        public async Task<bool> MockDelete(int id)
        {
            try
            {
                Tenant? item = await _context.Tenant.FindAsync(id);
                if (item != null)
                {
                    item.Delete = true;
                    _context.Update(item);

                    await _context.RACC
                        .Where(w => w.TenantId == id)
                        .ExecuteUpdateAsync(w => w.SetProperty(o => o.Delete, true));
                    await _context.UACC
                        .Where(w => w.TenantId == id)
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
                Tenant? item = await _context.Tenant.FindAsync(id);
                if (item != null)
                {
                    item.Delete = false;
                    _context.Update(item);

                    await _context.RACC
                        .Where(w => w.TenantId == id)
                        .ExecuteUpdateAsync(w => w.SetProperty(o => o.Delete, false));
                    await _context.UACC
                        .Where(w => w.TenantId == id)
                        .ExecuteUpdateAsync(w => w.SetProperty(o => o.Delete, false));
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        bool ICommon<Tenant>.Delete(Tenant item)
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

        IQueryable<Tenant> ICommon<Tenant>.GetAll()
        {
            return _context.Tenant
                .AsQueryable();
        }

        async Task<Tenant?> ICommon<Tenant>.GetByID(int id)
        {
            Tenant? item = await _context.Tenant
                .FirstOrDefaultAsync(w => w.Id == id);
            if (item != null)
            {
                return item;
            }
            return null;
        }

        async Task<bool> ICommon<Tenant>.Insert(Tenant item)
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

        bool ICommon<Tenant>.Update(Tenant item)
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
