using System.Diagnostics.CodeAnalysis;
using Manage.Data.Public;

namespace Manage.Data.Reminder.DTO.UserSubscription
{

    public record UserSubscriptionList
    {
        [SetsRequiredMembers]
        public UserSubscriptionList(Models.UserSubscription userSubscription)
        {
            Id = userSubscription.Id;
            UserId = userSubscription.UserId;
            SubTitle = userSubscription.Subscription.Title;
            Status = userSubscription.Status.ToString();
            Unavailable = userSubscription.Delete;
            CreateDateShamsi = DateDifference.MiladiToShamsi(userSubscription.CreateDate);
            LastUpdateDateShamsi = DateDifference.MiladiToShamsi(userSubscription.LastUpdateDate);
        }
        public int Id { get; init; }
        public int UserId { get; init; }
        public required string SubTitle { get; init; }
        public string Status { get; set; }
        public bool Unavailable { get; init; } = false;
        public required string CreateDateShamsi { get; init; }
        public required string LastUpdateDateShamsi { get; init; }
    }
    public record UserSubscriptionOne
    {
        [SetsRequiredMembers]
        public UserSubscriptionOne(Models.UserSubscription userSubscription)
        {
            Id = userSubscription.Id;
            UserId = userSubscription.UserId;
            SubTitle = userSubscription.Subscription.Title;
            Status = userSubscription.Status.ToString();
            Unavailable = userSubscription.Delete;
            ReminderLeftCount = userSubscription.Subscription.ReminderLimit - userSubscription.Reminders.Count();
            RemindersId = userSubscription.Reminders.OrderBy(w=>w.Date).Select(w => w.Id).ToArray();
            CreateDateShamsi = DateDifference.MiladiToShamsi(userSubscription.CreateDate);
            LastUpdateDateShamsi = DateDifference.MiladiToShamsi(userSubscription.LastUpdateDate);
        }
        public int Id { get; init; }
        public int UserId { get; init; }
        public required string SubTitle { get; init; }
        public string Status { get; set; }
        public int ReminderLeftCount { get; set; }
        public int[] RemindersId { get; set; }
        public bool Unavailable { get; init; } = false;
        public required string CreateDateShamsi { get; init; }
        public required string LastUpdateDateShamsi { get; init; }
    }

}
