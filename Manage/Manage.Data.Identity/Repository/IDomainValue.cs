using Microsoft.EntityFrameworkCore;
using Manage.Data.Identity.Models;

namespace Manage.Data.Identity.Repository
{
    public interface IDomainValue : ICommon<DomainValue>
    {
        IQueryable<DomainValue> GetByParent(int parentid);
        IQueryable<DomainValue> GetByDomain(int domainid);
        IQueryable<DomainValue> GetParents();
    }
    public class DomainValueEF : IDomainValue
    {
        private readonly Context _context;
        public DomainValueEF(Context context)
        {
            _context = context;
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                DomainValue? item = await _context.DomainValue.FindAsync(id);
                if (item != null)
                {
                    _context.SubDomainValue.Where(w => w.ChildId == id).ExecuteDelete();
                    _context.DomainValue.Remove(item);
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public IQueryable<DomainValue> GetAllNoTrack()
        {
            return _context.DomainValue
                .AsNoTracking()
                .Include(w => w.AsChild)
                .ThenInclude(c => c.Parent)
                .Include(w => w.Domain)
                .AsQueryable();
        }

        public IQueryable<DomainValue> GetByDomain(int domainid)
        {
            return _context.DomainValue
                .AsNoTracking()
                .Include(w => w.AsChild)
                .ThenInclude(c => c.Parent)
                .Include(w => w.Domain)
                .Where(w => w.DomainId == domainid)
                .AsQueryable();
        }

        public IQueryable<DomainValue> GetByParent(int parentid)
        {
            return _context.DomainValue
                .AsNoTracking()
                .Include(w => w.AsChild)
                .ThenInclude(c => c.Parent)
                .Include(w => w.Domain)
                .Where(w => w.AsChild.Any(e => e.ParentId == parentid))
                .AsQueryable();
        }

        public IQueryable<DomainValue> GetParents()
        {
            return _context.DomainValue
                .AsNoTracking()
                .Include(w => w.AsChild)
                .ThenInclude(c => c.Parent)
                .Where(w => w.AsParent.Count() > 0 && w.AsChild.Count() == 0)
                .AsQueryable();
        }

        public async Task<bool> MockDelete(int id)
        {
            try
            {
                DomainValue? item = await _context.DomainValue.FindAsync(id);
                if (item != null)
                {
                    item.Delete = true;
                    _context.Update(item);

                    var list = _context.DomainValue.Where(w => w.AsChild.Any(e => e.ParentId == item.Id));
                    foreach (var child in list)
                    {
                        await MockDelete(child.Id);
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
                DomainValue? item = await _context.DomainValue.FindAsync(id);
                if (item != null)
                {
                    item.Delete = false;
                    _context.Update(item);

                    var list = _context.DomainValue.Where(w => w.AsChild.Any(e => e.ParentId == item.Id));
                    foreach (var child in list)
                    {
                        await UnMockDelete(child.Id);
                    }

                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        bool ICommon<DomainValue>.Delete(DomainValue item)
        {
            try
            {
                _context.SubDomainValue.Where(w => w.ChildId == item.Id).ExecuteDelete();
                _context.Remove(item);
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        IQueryable<DomainValue> ICommon<DomainValue>.GetAll()
        {
            return _context.DomainValue
                .Include(w => w.AsChild)
                .ThenInclude(c => c.Parent)
                .Include(w => w.Domain)
                .AsQueryable();
        }

        async Task<DomainValue?> ICommon<DomainValue>.GetByID(int id)
        {
            DomainValue? item = await _context.DomainValue
                .Include(w => w.AsChild)
                .ThenInclude(c => c.Parent)
                .Include(w => w.AsParent)
                .ThenInclude(p => p.Child)
                .AsSplitQuery()
                .FirstOrDefaultAsync(w => w.Id == id);
            if (item != null)
            {
                return item;
            }
            return null;
        }

        async Task<bool> ICommon<DomainValue>.Insert(DomainValue item)
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

        bool ICommon<DomainValue>.Update(DomainValue item)
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
