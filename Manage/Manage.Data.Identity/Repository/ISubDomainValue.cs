using Microsoft.EntityFrameworkCore;
using Manage.Data.Identity.Models;

namespace Manage.Data.Identity.Repository
{
    public interface ISubDomainValue
    {
        Task<bool> AddChildToParent(int parentid, int[] childid, bool append = false);
    }
    public class SubDomainValueEF : ISubDomainValue
    {
        private readonly Context _context;
        public SubDomainValueEF(Context context)
        {
            _context = context;
        }

        public async Task<bool> AddChildToParent(int parentid, int[] childid, bool append = false)
        {
            try
            {
                var parentItems = _context.SubDomainValue.Where(w => w.ParentId == parentid);
                if (!append)
                {
                    _context.SubDomainValue
                        .Where(w => !childid.Any(c => c == w.ChildId))
                        .ExecuteDelete();
                }
                foreach (var item in childid)
                {
                    if (!parentItems.Any(w => w.ChildId == item) && _context.DomainValue.Find(item) != null)
                    {
                        await _context.SubDomainValue.AddAsync(new SubDomainValue { ParentId = parentid, ChildId = item });
                    }
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }
    }
}
