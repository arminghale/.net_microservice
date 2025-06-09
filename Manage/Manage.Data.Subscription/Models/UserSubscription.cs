using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manage.Data.Reminder.Models
{
    public class UserSubscription
    {
        public UserSubscription()
        {
            
        }
        [Key]
        public int Id { get; set; }
        [ForeignKey("Subscription")]
        public int SubscriptionId { get; set; }
        public int UserId { get; set; }
        public required SubscriptionStatus Status { get; set; } = SubscriptionStatus.PENDING;
        
        public bool Delete { get; set; } = false;

        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime LastUpdateDate { get; set; } = DateTime.Now;
        public virtual Subscription? Subscription { get; set; }
        public virtual ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();

    }
    public enum SubscriptionStatus
    {
        PENDING,
        ACCEPTED,
        REJECTED,
        CANCELED
    }
}
