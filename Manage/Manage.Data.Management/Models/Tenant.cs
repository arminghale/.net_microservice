using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Manage.Data.Management.Models
{
    public class Tenant
    {
        public Tenant()
        {
        }
        [Key]
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Additional { get; set; } = JsonSerializer.Serialize(new { });
        public required string RegisterRoles { get; set; } // , seprator
        public required string AdminRoles { get; set; } // , seprator
        public bool Delete { get; set; } = false;

        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime LastUpdateDate { get; set; } = DateTime.Now;

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<UACC> UACCs { get; set; } = new List<UACC>();
        public virtual ICollection<RACC> RACCs { get; set; } = new List<RACC>();
    }
}
