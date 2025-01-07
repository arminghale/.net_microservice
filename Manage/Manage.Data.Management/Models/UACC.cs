using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manage.Data.Management.Models
{
    public class UACC
    {
        public UACC()
        {

        }
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        [ForeignKey("Action")]
        public int ActionId { get; set; }
        [ForeignKey("Tenant")]
        public int? TenantId { get; set; }
        public int type { get; set; } = 0; // 0 has access, 1 no access
        public bool Delete { get; set; } = false;
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime LastUpdateDate { get; set; } = DateTime.Now;
        public virtual User? User { get; set; }
        public virtual Action? Action { get; set; }
        public virtual Tenant? Tenant { get; set; }

    }
}
