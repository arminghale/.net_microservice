using Manage.Data.Reminder.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Manage.Data.Reminder.Repository
{
    public interface ISubscription : ICommon<Subscription>
    {
        IQueryable<Subscription> GetByTenant(int tenantId);
        Task<Subscription> GetByTitle(string title);
    }
    public class SubscriptionEF : ISubscription
    {
        private readonly Context _context;

        public SubscriptionEF(Context context)
        {
            _context = context;
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                Subscription? item = await _context.Subscription.FindAsync(id);
                if (item != null)
                {
                    _context.Subscription.Remove(item);
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public async Task<bool> MockDelete(int id)
        {
            try
            {
                Subscription? item = await _context.Subscription.FindAsync(id);
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

        bool ICommon<Subscription>.Delete(Subscription item)
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

        IQueryable<Subscription> ICommon<Subscription>.GetAll()
        {
            return _context.Subscription
                .AsQueryable();
        }

        async Task<Subscription?> ICommon<Subscription>.GetByID(int id)
        {
            return await _context.Subscription
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        async Task<bool> ICommon<Subscription>.Insert(Subscription item)
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

        bool ICommon<Subscription>.Update(Subscription item)
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

        public async Task<bool> UnMockDelete(int id)
        {
            try
            {
                Subscription? item = await _context.Subscription.FindAsync(id);
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

        public IQueryable<Subscription> GetAllNoTrack()
        {
            return _context.Subscription
                .AsNoTracking()
                .AsQueryable();
        }

        public async Task<Subscription?> GetByTitle(string title)
        {
            return await _context.Subscription.FirstOrDefaultAsync(w => string.Equals(w.Title.ToLower(), title.ToLower()));
        }

        public IQueryable<Subscription> GetByTenant(int tenantId)
        {
            return _context.Subscription
                .Where(w=> w.TenantId == tenantId)
                .AsNoTracking()
                .AsQueryable();
        }
    }
}
