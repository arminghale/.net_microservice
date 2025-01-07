using Microsoft.EntityFrameworkCore;
using Manage.Data.Management.Models;

namespace Manage.Data.Management.Repository
{
    public interface IDomain : ICommon<Domain>
    {
        Task<Domain?> GetByTitle(string title);
    }
    public class DomainEF : IDomain
    {
        private readonly Context _context;
        private readonly IDomainValue _domainValue;
        public DomainEF(Context context, IDomainValue domainValue)
        {
            _context = context;
            _domainValue = domainValue;
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                Domain? item = await _context.Domain.FindAsync(id);
                if (item != null)
                {
                    var domainvaluesIds = _context.DomainValue.Where(w => w.DomainId == item.Id).Select(w => w.Id);
                    _context.SubDomainValue.Where(w => domainvaluesIds.Any(e => e == w.ChildId)).ExecuteDelete();
                    _context.Domain.Remove(item);
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public IQueryable<Domain> GetAllNoTrack()
        {
            return _context.Domain.AsNoTracking().AsQueryable();
        }

        public async Task<Domain?> GetByTitle(string title)
        {
            return await _context.Domain
                .Include(w => w.DomainValues)
                .AsSplitQuery()
                .FirstOrDefaultAsync(w => string.Equals(w.Title, title));
        }

        public async Task<bool> MockDelete(int id)
        {
            try
            {
                Domain? item = await _context.Domain.FindAsync(id);
                if (item != null)
                {
                    item.Delete = true;
                    _context.Update(item);

                    foreach (var domainvalue in item.DomainValues)
                    {
                        await _domainValue.MockDelete(domainvalue.Id);
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
                Domain? item = await _context.Domain.FindAsync(id);
                if (item != null)
                {
                    item.Delete = false;
                    _context.Update(item);

                    foreach (var domainvalue in item.DomainValues)
                    {
                        await _domainValue.UnMockDelete(domainvalue.Id);
                    }

                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        bool ICommon<Domain>.Delete(Domain item)
        {
            try
            {
                var domainvaluesIds = _context.DomainValue.Where(w => w.DomainId == item.Id).Select(w => w.Id);
                _context.SubDomainValue.Where(w => domainvaluesIds.Any(e => e == w.ChildId)).ExecuteDelete();
                _context.Remove(item);
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        IQueryable<Domain> ICommon<Domain>.GetAll()
        {
            return _context.Domain.AsQueryable();
        }

        async Task<Domain?> ICommon<Domain>.GetByID(int id)
        {
            Domain? item = await _context.Domain
                .Include(w => w.DomainValues)
                .AsSplitQuery()
                .FirstOrDefaultAsync(w => w.Id == id);
            if (item != null)
            {
                return item;
            }
            return null;
        }

        async Task<bool> ICommon<Domain>.Insert(Domain item)
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

        bool ICommon<Domain>.Update(Domain item)
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
