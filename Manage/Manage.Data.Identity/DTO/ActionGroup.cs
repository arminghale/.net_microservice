using System.Diagnostics.CodeAnalysis;
using Manage.Data.Identity.DTO.Action;
using Manage.Data.Public;

namespace Manage.Data.Identity.DTO.ActionGroup
{

    public record ActionGroupList
    {
        [SetsRequiredMembers]
        public ActionGroupList(Models.ActionGroup actiongroup)
        {
            Id = actiongroup.Id;
            ServiceId = actiongroup.ServiceId;
            Title = actiongroup.Title;
            Unavailable = actiongroup.Delete;
            CreateDateShamsi = DateDifference.MiladiToShamsi(actiongroup.CreateDate);
            LastUpdateDateShamsi = DateDifference.MiladiToShamsi(actiongroup.LastUpdateDate);
        }
        public int Id { get; init; }
        public int ServiceId { get; init; }
        public required string Title { get; init; }
        public bool Unavailable { get; init; } = false;
        public required string CreateDateShamsi { get; init; }
        public required string LastUpdateDateShamsi { get; init; }
    }
    public record ActionGroupOne
    {
        public ActionGroupOne(Models.ActionGroup actiongroup)
        {
            ActionGroup = new ActionGroupList(actiongroup);
            Actions = actiongroup.Actions.Select(w => new ActionList(w));
        }
        public ActionGroupList ActionGroup { get; init; }
        public IEnumerable<ActionList> Actions { get; init; }
    }
}
