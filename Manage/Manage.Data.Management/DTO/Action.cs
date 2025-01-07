using System.Diagnostics.CodeAnalysis;
using Manage.Data.Public;

namespace Manage.Data.Management.DTO.Action
{

    public record ActionList
    {
        [SetsRequiredMembers]
        public ActionList(Models.Action action)
        {
            Id = action.Id;
            ActionGroupId = action.ActionGroupId;
            Title = action.Title;
            URL = action.URL;
            Type = action.Type;
            Unavailable = action.Delete;
            CreateDateShamsi = DateDifference.MiladiToShamsi(action.CreateDate);
            LastUpdateDateShamsi = DateDifference.MiladiToShamsi(action.LastUpdateDate);
        }
        public int Id { get; init; }
        public int ActionGroupId { get; init; }
        public required string Title { get; init; }
        public required string URL { get; init; }
        public required string Type { get; init; }
        public bool Unavailable { get; init; } = false;
        public required string CreateDateShamsi { get; init; }
        public required string LastUpdateDateShamsi { get; init; }
    }
}
