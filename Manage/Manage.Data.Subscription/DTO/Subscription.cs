using System.Diagnostics.CodeAnalysis;
using Manage.Data.Public;

namespace Manage.Data.Reminder.DTO.Subscription
{

    public record SubscriptionList
    {
        [SetsRequiredMembers]
        public SubscriptionList(Models.Subscription subscription)
        {
            Id = subscription.Id;
            Title = subscription.Title;
            ReminderLimit = subscription.ReminderLimit;
            Unavailable = subscription.Delete;
            CreateDateShamsi = DateDifference.MiladiToShamsi(subscription.CreateDate);
            LastUpdateDateShamsi = DateDifference.MiladiToShamsi(subscription.LastUpdateDate);
        }
        public int Id { get; init; }
        public required string Title { get; init; }
        public int ReminderLimit { get; set; }
        public bool Unavailable { get; init; } = false;
        public required string CreateDateShamsi { get; init; }
        public required string LastUpdateDateShamsi { get; init; }
    }
    public record SubscriptionOne
    {
        [SetsRequiredMembers]
        public SubscriptionOne(Models.Subscription subscription)
        {
            Id = subscription.Id;
            Title = subscription.Title;
            ReminderLimit = subscription.ReminderLimit;
            Unavailable = subscription.Delete;
            SubUsers = subscription.UserSubscriptions.Count;
            CreateDateShamsi = DateDifference.MiladiToShamsi(subscription.CreateDate);
            LastUpdateDateShamsi = DateDifference.MiladiToShamsi(subscription.LastUpdateDate);
        }
        public int Id { get; init; }
        public required string Title { get; init; }
        public int ReminderLimit { get; set; }
        public required int SubUsers { get; init; }
        public bool Unavailable { get; init; } = false;
        public required string CreateDateShamsi { get; init; }
        public required string LastUpdateDateShamsi { get; init; }
    }

}
