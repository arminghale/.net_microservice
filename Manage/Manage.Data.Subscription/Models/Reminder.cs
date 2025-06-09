using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manage.Data.Reminder.Models
{
    public class Reminder
    {
        public Reminder()
        {
            
        }
        [Key]
        public int Id { get; set; }
        [ForeignKey("UserSubscription")]
        public int UserSubscriptionId { get; set; }
        public required DateTime Date { get; set; } = DateTime.Now;
        public required string Title { get; set; }
        public string Description { get; set; }
        public ReminderStatus Status { get; set; } = ReminderStatus.NoDone;
        public bool Delete { get; set; } = false;

        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime LastUpdateDate { get; set; } = DateTime.Now;

        public virtual UserSubscription? UserSubscription { get; set; }

    }
    public enum ReminderStatus
    {
        NoDone,
        Done,
        ReScheduled
    }
}
