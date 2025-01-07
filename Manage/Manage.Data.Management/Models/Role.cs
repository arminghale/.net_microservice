using System.ComponentModel.DataAnnotations;

namespace Manage.Data.Management.Models
{
    public class Role
    {
        public Role()
        {

        }
        [Key]
        public int Id { get; set; }
        public required string Title { get; set; }
        public bool Delete { get; set; } = false;
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime LastUpdateDate { get; set; } = DateTime.Now;

        public virtual IEnumerable<UserRole> UserRoles { get; set; } = Enumerable.Empty<UserRole>();
        public virtual IEnumerable<RACC> RACCs { get; set; } = Enumerable.Empty<RACC>();

    }
}
