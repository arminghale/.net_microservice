using System.Diagnostics.CodeAnalysis;
using Manage.Data.Public;
using Manage.Data.Reminder.Models;

namespace Manage.Data.Reminder.DTO.Reminder
{

    public record ReminderList
    {
        [SetsRequiredMembers]
        public ReminderList(Models.Reminder reminder)
        {
            Id = reminder.Id;
            Title = reminder.Title;
            Description = reminder.Description;
            Status = reminder.Status;
            DateShamsi = DateDifference.MiladiToShamsi(reminder.Date);
            Unavailable = reminder.Delete;
            CreateDateShamsi = DateDifference.MiladiToShamsi(reminder.CreateDate);
            LastUpdateDateShamsi = DateDifference.MiladiToShamsi(reminder.LastUpdateDate);
        }
        public int Id { get; init; }
        public required string Title { get; init; }
        public string Description { get; init; }
        public ReminderStatus Status { get; set; }
        public required string DateShamsi { get; init; }
        public bool Unavailable { get; init; } = false;
        public required string CreateDateShamsi { get; init; }
        public required string LastUpdateDateShamsi { get; init; }
    }
    public record ReminderOne
    {
        [SetsRequiredMembers]
        public ReminderOne(Models.Reminder reminder)
        {
            Id = reminder.Id;
            SubscriptionId = reminder.UserSubscription.SubscriptionId;
            UserSubscriptionId = reminder.UserSubscriptionId;
            SubscriptionTitle = reminder.UserSubscription.Subscription.Title;
            Title = reminder.Title;
            Description = reminder.Description;
            Status = reminder.Status;
            DateShamsi = DateDifference.MiladiToShamsi(reminder.Date);
            Unavailable = reminder.Delete;
            CreateDateShamsi = DateDifference.MiladiToShamsi(reminder.CreateDate);
            LastUpdateDateShamsi = DateDifference.MiladiToShamsi(reminder.LastUpdateDate);
        }
        public int Id { get; init; }
        public int SubscriptionId { get; init; }
        public int UserSubscriptionId { get; init; }
        public required string SubscriptionTitle { get; init; }
        public required string Title { get; init; }
        public string Description { get; init; }
        public ReminderStatus Status { get; set; }
        public required string DateShamsi { get; init; }
        public bool Unavailable { get; init; } = false;
        public required string CreateDateShamsi { get; init; }
        public required string LastUpdateDateShamsi { get; init; }
    }

}
