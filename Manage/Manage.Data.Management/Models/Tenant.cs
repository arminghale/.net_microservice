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
        public required string RegisterRoles { get; set; }
        public required string AdminRoles { get; set; }
        public bool Delete { get; set; } = false;

        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime LastUpdateDate { get; set; } = DateTime.Now;

        public virtual IEnumerable<UserRole> UserRoles { get; set; } = Enumerable.Empty<UserRole>();
        public virtual IEnumerable<UACC> UACCs { get; set; } = Enumerable.Empty<UACC>();
        public virtual IEnumerable<RACC> RACCs { get; set; } = Enumerable.Empty<RACC>();
    }
}
