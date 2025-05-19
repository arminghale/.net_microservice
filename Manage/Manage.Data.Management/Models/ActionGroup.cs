using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manage.Data.Management.Models
{
    public class ActionGroup
    {
        public ActionGroup()
        {
        }
        [Key]
        public int Id { get; set; }
        [ForeignKey("Service")]
        public int ServiceId { get; set; }
        public required string Title { get; set; }
        public bool Delete { get; set; } = false;

        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime LastUpdateDate { get; set; } = DateTime.Now;

        public virtual Service? Service { get; set; }
        public virtual ICollection<Action> Actions { get; set; } = new List<Action>();
    }
}
