using Microsoft.EntityFrameworkCore;
using Manage.Data.Identity.Models;

namespace Manage.Data.Identity.Repository
{
    public interface IService : ICommon<Service>
    {
        Task<Service?> GetByTitle(string title);
    }
    public class ServiceEF : IService
    {
        private readonly Context _context;
        private readonly IActionGroup _actionGroup;
        public ServiceEF(Context context, IActionGroup actionGroup)
        {
            _context = context;
            _actionGroup = actionGroup;
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                Service? item = await _context.Service.FindAsync(id);
                if (item != null)
                {
                    foreach (var ag in _context.ActionGroup.Where(w => w.ServiceId == id))
                    {
                        await _actionGroup.Delete(ag.Id);
                    }
                    _context.Service.Remove(item);
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public IQueryable<Service> GetAllNoTrack()
        {
            return _context.Service.AsNoTracking().AsQueryable();
        }


        public async Task<Service?> GetByTitle(string title)
        {
            return await _context.Service
                .AsSplitQuery()
                .FirstOrDefaultAsync(w => string.Equals(w.Title, title));
        }

        public async Task<bool> MockDelete(int id)
        {
            try
            {
                Service? item = await _context.Service.FindAsync(id);
                if (item != null)
                {
                    item.Delete = true;
                    _context.Update(item);

                    foreach (var ag in _context.ActionGroup.Where(w => w.ServiceId == id))
                    {
                        await _actionGroup.MockDelete(ag.Id);
                    }

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
                Service? item = await _context.Service.FindAsync(id);
                if (item != null)
                {
                    item.Delete = false;
                    _context.Update(item);

                    foreach (var ag in _context.ActionGroup.Where(w => w.ServiceId == id))
                    {
                        await _actionGroup.UnMockDelete(ag.Id);
                    }

                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        bool ICommon<Service>.Delete(Service item)
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

        IQueryable<Service> ICommon<Service>.GetAll()
        {
            return _context.Service.AsQueryable();
        }

        async Task<Service?> ICommon<Service>.GetByID(int id)
        {
            Service? item = await _context.Service
                .AsSplitQuery()
                .FirstOrDefaultAsync(w => w.Id == id);
            if (item != null)
            {
                return item;
            }
            return null;
        }

        async Task<bool> ICommon<Service>.Insert(Service item)
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

        bool ICommon<Service>.Update(Service item)
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
