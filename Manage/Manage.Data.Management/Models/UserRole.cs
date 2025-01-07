using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manage.Data.Management.Models
{
    public class UserRole
    {
        public UserRole()
        {

        }
        [Key]
        public int Id { get; set; }
        [ForeignKey("Role")]
        public int RoleId { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        [ForeignKey("Tenant")]
        public int? TenantId { get; set; }
        public bool Delete { get; set; } = false;
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime LastUpdateDate { get; set; } = DateTime.Now;

        public virtual User? User { get; set; }
        public virtual Role? Role { get; set; }
        public virtual Tenant? Tenant { get; set; }
    }
}
