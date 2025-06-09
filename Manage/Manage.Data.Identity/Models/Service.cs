using System.ComponentModel.DataAnnotations;

namespace Manage.Data.Identity.Models
{
    public class Service
    {
        public Service()
        {
        }
        [Key]
        public int Id { get; set; }
        public required string Title { get; set; }
        public bool Delete { get; set; } = false;

        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime LastUpdateDate { get; set; } = DateTime.Now;

        public virtual ICollection<ActionGroup> ActionGroups { get; set; } = new List<ActionGroup>();
    }
}
