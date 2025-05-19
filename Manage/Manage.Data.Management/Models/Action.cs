using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manage.Data.Management.Models
{
    public class Action
    {
        public Action()
        {
        }
        [Key]
        public int Id { get; set; }
        [ForeignKey("ActionGroup")]
        public int ActionGroupId { get; set; }
        public required string Title { get; set; } // /api/v1/...
        public required string URL { get; set; }
        public required string Type { get; set; } // POST GET PUT DELETE
        public bool Delete { get; set; } = false;

        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime LastUpdateDate { get; set; } = DateTime.Now;
        public virtual ActionGroup? ActionGroup { get; set; }
        public virtual ICollection<RACC>? RACCs { get; set; } = new List<RACC>();
        public virtual ICollection<UACC>? UACCs { get; set; } = new List<UACC>();
    }
}
