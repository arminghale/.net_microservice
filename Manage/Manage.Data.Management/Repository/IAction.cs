using Manage.Data.Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Manage.Data.Management.Repository
{
    public interface IAction : ICommon<Models.Action>
    {
        IQueryable<Models.Action> GetByActionGroup(int actiongroupid);
        IQueryable<Models.Action> GetByService(int serviceid);
        Task<Models.Action?> GetByTitleAndActionGroup(string title,int actiongroupid);
        Task<Models.Action?> GetByURLAndActionGroup(string url,int actiongroupid);
    }
    public class ActionEF : IAction
    {
        private readonly Context _context;
        public ActionEF(Context context)
        {
            _context = context;
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                Models.Action? item = await _context.Action.FindAsync(id);
                if (item != null)
                {
                    _context.RACC.Where(w => w.ActionId == id).ExecuteDelete();
                    _context.UACC.Where(w => w.ActionId == id).ExecuteDelete();
                    _context.Action.Remove(item);
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public IQueryable<Models.Action> GetAllNoTrack()
        {
            return _context.Action
                .AsNoTracking()
                .Include(w => w.ActionGroup)
                .AsQueryable();
        }

        public IQueryable<Models.Action> GetByActionGroup(int actiongroupid)
        {
            return _context.Action
                .AsNoTracking()
                .Include(w => w.ActionGroup)
                .Where(w => w.ActionGroupId == actiongroupid)
                .AsQueryable();
        }

        public IQueryable<Models.Action> GetByService(int serviceid)
        {
            return _context.Action
                .AsNoTracking()
                .Include(w => w.ActionGroup)
                .Where(w => w.ActionGroup.ServiceId == serviceid)
                .AsQueryable();
        }
        public async Task<Models.Action?> GetByTitleAndActionGroup(string title, int actiongroupid)
        {
            return await _context.Action
                .AsNoTracking()
                .Where(w => w.ActionGroupId == actiongroupid)
                .FirstOrDefaultAsync(w => string.Equals(w.Title, title));
        }
        public async Task<Models.Action?> GetByURLAndActionGroup(string url, int actiongroupid)
        {
            return await _context.Action
                .AsNoTracking()
                .Where(w => w.ActionGroupId == actiongroupid)
                .FirstOrDefaultAsync(w => string.Equals(w.URL, url));
        }

        public async Task<bool> MockDelete(int id)
        {
            try
            {
                Models.Action? item = await _context.Action.FindAsync(id);
                if (item != null)
                {
                    item.Delete = true;
                    _context.Update(item);

                    await _context.RACC
                        .Where(w => w.ActionId == id)
                        .ExecuteUpdateAsync(w => w.SetProperty(o => o.Delete, true));
                    await _context.UACC
                        .Where(w => w.ActionId == id)
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
                Models.Action? item = await _context.Action.FindAsync(id);
                if (item != null)
                {
                    item.Delete = false;
                    _context.Update(item);

                    await _context.RACC
                        .Where(w => w.ActionId == id)
                        .ExecuteUpdateAsync(w => w.SetProperty(o => o.Delete, false));
                    await _context.UACC
                        .Where(w => w.ActionId == id)
                        .ExecuteUpdateAsync(w => w.SetProperty(o => o.Delete, false));

                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        bool ICommon<Models.Action>.Delete(Models.Action item)
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

        IQueryable<Models.Action> ICommon<Models.Action>.GetAll()
        {
            return _context.Action
                .Include(w => w.ActionGroup)
                .AsQueryable();
        }

        async Task<Models.Action?> ICommon<Models.Action>.GetByID(int id)
        {
            Models.Action? item = await _context.Action
                .Include(w => w.ActionGroup)
                .AsSplitQuery()
                .FirstOrDefaultAsync(w => w.Id == id);
            if (item != null)
            {
                return item;
            }
            return null;
        }

        async Task<bool> ICommon<Models.Action>.Insert(Models.Action item)
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

        bool ICommon<Models.Action>.Update(Models.Action item)
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
