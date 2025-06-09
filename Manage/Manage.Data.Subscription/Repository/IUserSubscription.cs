using Manage.Data.Reminder.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Manage.Data.Reminder.Repository
{
    public interface IUserSubscription : ICommon<UserSubscription>
    {
        IQueryable<UserSubscription> GetByUser(int userid);
        IQueryable<UserSubscription> GetByTenant(int tenantid);
        IQueryable<UserSubscription> GetBySubscription(int subscriptionid);
    }
    public class UserSubscriptionEF : IUserSubscription
    {
        private readonly Context _context;

        public UserSubscriptionEF(Context context)
        {
            _context = context;
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                UserSubscription? item = await _context.UserSubscription.FindAsync(id);
                if (item != null)
                {
                    _context.UserSubscription.Remove(item);
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
                UserSubscription? item = await _context.UserSubscription.FindAsync(id);
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

        bool ICommon<UserSubscription>.Delete(UserSubscription item)
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

        IQueryable<UserSubscription> ICommon<UserSubscription>.GetAll()
        {
            return _context.UserSubscription
                .Include(w => w.Subscription)
                .AsQueryable();
        }

        async Task<UserSubscription?> ICommon<UserSubscription>.GetByID(int id)
        {
            return await _context.UserSubscription
                .Include(w => w.Subscription)
                .Include(w=>w.Reminders)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        async Task<bool> ICommon<UserSubscription>.Insert(UserSubscription item)
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

        bool ICommon<UserSubscription>.Update(UserSubscription item)
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
                UserSubscription? item = await _context.UserSubscription.FindAsync(id);
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

        public IQueryable<UserSubscription> GetAllNoTrack()
        {
            return _context.UserSubscription
                .Include(w => w.Subscription)
                .AsNoTracking()
                .AsQueryable();
        }

        public IQueryable<UserSubscription> GetByUser(int userid)
        {
            return _context.UserSubscription
                    .Include(w => w.Subscription)
                    .Include(w => w.Reminders)
                    .Where(w=> w.UserId == userid)
                    .AsQueryable();
        }

        public IQueryable<UserSubscription> GetBySubscription(int subscriptionid)
        {
            return _context.UserSubscription
                    .Include(w => w.Subscription)
                    .Where(w => w.SubscriptionId == subscriptionid)
                    .AsQueryable();
        }

        public IQueryable<UserSubscription> GetByTenant(int tenantid)
        {
            return _context.UserSubscription
                    .Include(w => w.Subscription)
                    .Where(w => w.Subscription.TenantId == tenantid)
                    .AsQueryable();
        }
    }
}
