using Microsoft.EntityFrameworkCore;
using Manage.Data.Management.Models;

namespace Manage.Data.Management.Repository
{
    public interface IActionGroup : ICommon<ActionGroup>
    {
        IQueryable<ActionGroup> GetByService(int serviceid);
        Task<ActionGroup?> GetByTitleAndService(string title, int serviceid);
    }
    public class ActionGroupEF : IActionGroup
    {
        private readonly Context _context;
        private readonly IAction _action;
        public ActionGroupEF(Context context, IAction action)
        {
            _context = context;
            _action = action;
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                ActionGroup? item = await _context.ActionGroup.FindAsync(id);
                if (item != null)
                {
                    foreach (var action in _context.Action.Where(w => w.ActionGroupId == id))
                    {
                        await _action.Delete(action.Id);
                    }
                    _context.ActionGroup.Remove(item);
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public IQueryable<ActionGroup> GetAllNoTrack()
        {
            return _context.ActionGroup
                .AsNoTracking()
                .Include(w => w.Actions)
                .AsQueryable();
        }

        public IQueryable<ActionGroup> GetByService(int serviceid)
        {
            return _context.ActionGroup
                .AsNoTracking()
                .Include(w => w.Actions)
                .Where(w => w.ServiceId == serviceid)
                .AsQueryable();
        }

        public async Task<ActionGroup?> GetByTitleAndService(string title, int serviceid)
        {
            return await _context.ActionGroup
                .AsNoTracking()
                .Include(w => w.Actions)
                .Where(w => w.ServiceId == serviceid)
                .FirstOrDefaultAsync(w => string.Equals(w.Title, title));
        }

        public async Task<bool> MockDelete(int id)
        {
            try
            {
                ActionGroup? item = await _context.ActionGroup.FindAsync(id);
                if (item != null)
                {
                    item.Delete = true;
                    _context.Update(item);

                    foreach (var action in _context.Action.Where(w => w.ActionGroupId == id))
                    {
                        await _action.MockDelete(action.Id);
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
                ActionGroup? item = await _context.ActionGroup.FindAsync(id);
                if (item != null)
                {
                    item.Delete = false;
                    _context.Update(item);

                    foreach (var action in _context.Action.Where(w => w.ActionGroupId == id))
                    {
                        await _action.UnMockDelete(action.Id);
                    }

                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        bool ICommon<ActionGroup>.Delete(ActionGroup item)
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

        IQueryable<ActionGroup> ICommon<ActionGroup>.GetAll()
        {
            return _context.ActionGroup
                .Include(w => w.Actions)
                .AsQueryable();
        }

        async Task<ActionGroup?> ICommon<ActionGroup>.GetByID(int id)
        {
            ActionGroup? item = await _context.ActionGroup
                .Include(w => w.Actions)
                .FirstOrDefaultAsync(w => w.Id == id);
            if (item != null)
            {
                return item;
            }
            return null;
        }

        async Task<bool> ICommon<ActionGroup>.Insert(ActionGroup item)
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

        bool ICommon<ActionGroup>.Update(ActionGroup item)
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
