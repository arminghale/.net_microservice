using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manage.Data.Reminder.Models
{
    public class Subscription
    {
        public Subscription()
        {
            
        }
        [Key]
        public int Id { get; set; }
        public int TenantId { get; set; }
        public required string Title { get; set; }
        public int ReminderLimit { get; set; } = 3;
        public bool Delete { get; set; } = false;

        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime LastUpdateDate { get; set; } = DateTime.Now;
        public virtual ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();

    }
}
