using Manage.Data.Reminder.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Manage.Data.Reminder.Repository
{
    public interface IReminder : ICommon<Reminder.Models.Reminder>
    {
        IQueryable<Reminder.Models.Reminder> GetByTenant(int tenantid);
        IQueryable<Reminder.Models.Reminder> GetByUser(int userid);
        IQueryable<Reminder.Models.Reminder> GetBySubscription(int subscriptionid);
    }
    public class ReminderEF : IReminder
    {
        private readonly Context _context;

        public ReminderEF(Context context)
        {
            _context = context;
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                Reminder.Models.Reminder? item = await _context.Reminder.FindAsync(id);
                if (item != null)
                {
                    _context.Reminder.Remove(item);
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
                Reminder.Models.Reminder? item = await _context.Reminder.FindAsync(id);
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

        bool ICommon<Reminder.Models.Reminder>.Delete(Reminder.Models.Reminder item)
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

        IQueryable<Reminder.Models.Reminder> ICommon<Reminder.Models.Reminder>.GetAll()
        {
            return _context.Reminder
                .Include(w => w.UserSubscription)
                    .ThenInclude(us => us.Subscription)
                .AsQueryable();
        }

        async Task<Reminder.Models.Reminder?> ICommon<Reminder.Models.Reminder>.GetByID(int id)
        {
            return await _context.Reminder
                .Include(w => w.UserSubscription)
                    .ThenInclude(us => us.Subscription)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        async Task<bool> ICommon<Reminder.Models.Reminder>.Insert(Reminder.Models.Reminder item)
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

        bool ICommon<Reminder.Models.Reminder>.Update(Reminder.Models.Reminder item)
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
                Reminder.Models.Reminder? item = await _context.Reminder.FindAsync(id);
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

        public IQueryable<Reminder.Models.Reminder> GetAllNoTrack()
        {
            return _context.Reminder
                .Include(w => w.UserSubscription)
                    .ThenInclude(us => us.Subscription)
                .AsNoTracking()
                .AsQueryable();
        }

        public IQueryable<Reminder.Models.Reminder> GetByUser(int userid)
        {
            return _context.Reminder
                    .Include(w => w.UserSubscription)
                        .ThenInclude(us => us.Subscription)
                    .Where(w=> w.UserSubscription.UserId == userid)
                    .AsQueryable();
        }

        public IQueryable<Reminder.Models.Reminder> GetBySubscription(int subscriptionid)
        {
            return _context.Reminder
                    .Include(w => w.UserSubscription)
                        .ThenInclude(us => us.Subscription)
                    .Where(w => w.UserSubscription.SubscriptionId == subscriptionid)
                    .AsQueryable();
        }

        public IQueryable<Models.Reminder> GetByTenant(int tenantid)
        {
            return _context.Reminder
                    .Include(w => w.UserSubscription)
                        .ThenInclude(us => us.Subscription)
                    .Where(w => w.UserSubscription.Subscription.TenantId == tenantid)
                    .AsQueryable();
        }
    }
}
