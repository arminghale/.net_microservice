using Manage.Data.Identity.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace Manage.Data.Identity.Repository
{
    public interface IUser : ICommon<User>
    {
        IQueryable<User> GetByRole(int role);
        IQueryable<User> GetByTenant(int domainvalue);
        IQueryable<User> GetByValidation(bool validate);
        Task<User?> GetByPhonenumber(string phonenumber);
        Task<User?> GetByEmail(string email);
        Task<User?> GetByUsername(string username);
        Task<User?> GetByRefrence(string refrence);
        Task<User?> GetByToken(string token);
        Task<User?> CheckLogin(string username, string password);

        string TokenGenerator();
    }
    public class UserEF : IUser
    {
        private readonly Context _context;
        private readonly ICache _cache;
        private readonly int expirationMin = int.Parse(Environment.GetEnvironmentVariable("UserCacheExpirationMin"));

        public UserEF(Context context, ICache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<User?> CheckLogin(string username, string password)
        {
            User? user = null;
            if (new EmailAddressAttribute().IsValid(username))
            {
                user = await GetByEmail(username);
            }
            else if (new PhoneAttribute().IsValid(username))
            {
                user = await GetByPhonenumber(username);
            }
            else
            {
                user = await GetByUsername(username);
            }

            if (user == null)
            {
                return null;
            }

            if (!string.Equals(user.Password, password, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            return user;

        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                User? item = await _context.User.FindAsync(id);
                if (item != null)
                {
                    _context.User.Remove(item);
                    _cache.RemoveData($"USER_{item.Id}");
                }
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public IQueryable<User> GetByValidation(bool validate)
        {
            return _context.User
                .Where(w => validate ? w.Validation == "YES" : w.Validation == "NO")
                .AsQueryable();
        }

        public async Task<User?> GetByEmail(string email)
        {
            return await _context.User.FirstOrDefaultAsync(w => w.Email != null && string.Equals(w.Email.ToLower(), email.ToLower()));
        }

        public async Task<User?> GetByPhonenumber(string phonenumber)
        {
            return await _context.User.FirstOrDefaultAsync(w => string.Equals(w.Phonenumber, phonenumber));
        }

        public IQueryable<User> GetByRole(int role)
        {
            return _context.User
                .Include(w => w.UserRoles)
                .Where(w => w.UserRoles.Any(e => e.RoleId == role))
                .AsQueryable();
        }

        public async Task<User?> GetByUsername(string username)
        {
            return await _context.User.FirstOrDefaultAsync(w => string.Equals(w.Username.ToLower(), username.ToLower()));

        }

        public async Task<bool> MockDelete(int id)
        {
            try
            {
                User? item = await _context.User.FindAsync(id);
                if (item != null)
                {
                    item.Delete = true;
                    _context.Update(item);
                    _cache.RemoveData($"USER_{id}");
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

        bool ICommon<User>.Delete(User item)
        {
            try
            {
                _context.Remove(item);
                _cache.RemoveData($"USER_{item.Id}");
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        IQueryable<User> ICommon<User>.GetAll()
        {
            return _context.User
                .AsQueryable();
        }

        async Task<User?> ICommon<User>.GetByID(int id)
        {
            User? item = _cache.GetData<User>($"USER_{id}", expirationMin);
            if (item == null)
            {
                item = await _context.User
                    //.Include(w => w.UserRoles)
                        //.ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(w => w.Id == id);
                if (item == null)
                {
                    return null;
                }
                _cache.SetData<User>($"USER_{id}", item, expirationMin);
            }
            return item;  
        }

        async Task<bool> ICommon<User>.Insert(User item)
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

        bool ICommon<User>.Update(User item)
        {
            try
            {
                _context.Update(item);
                _cache.RemoveData($"USER_{item.Id}");
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
                User? item = await _context.User.FindAsync(id);
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

        public IQueryable<User> GetAllNoTrack()
        {
            return _context.User
                .AsNoTracking()
                .AsQueryable();
        }

        public IQueryable<User> GetByTenant(int domainvalue)
        {
            return _context.User
                .Include(w => w.UACCs)
                .Include(w => w.UserRoles)
                    .ThenInclude(r => r.Role)
                        .ThenInclude(rr => rr.RACCs)
                .Where(w => w.UserRoles.Any(e => e.TenantId == domainvalue || e.Role.RACCs.Any(d => d.TenantId == domainvalue))
                || w.UACCs.Any(e => e.TenantId == domainvalue))
                .AsSplitQuery()
                .AsQueryable();
        }

        public async Task<User?> GetByRefrence(string refrence)
        {
            return await _context.User.FirstOrDefaultAsync(w => string.Equals(w.RefrenceId.ToLower(), refrence.ToLower()));
        }

        public async Task<User?> GetByToken(string token)
        {
            return await _context.User.FirstOrDefaultAsync(w => string.Equals(w.Token.ToLower(), token.ToLower()));
        }

        public string TokenGenerator()
        {
            return Guid.NewGuid().ToString().Replace("-", "").ToUpper();
        }
    }
}
