using System.Diagnostics.CodeAnalysis;
using Manage.Data.Public;

namespace Manage.Data.Identity.DTO.Role
{

    public record RoleList
    {
        [SetsRequiredMembers]
        public RoleList(Models.Role role)
        {
            Id = role.Id;
            Title = role.Title;
            Unavailable = role.Delete;
            CreateDateShamsi = DateDifference.MiladiToShamsi(role.CreateDate);
            LastUpdateDateShamsi = DateDifference.MiladiToShamsi(role.LastUpdateDate);
        }
        public int Id { get; init; }
        public required string Title { get; init; }
        public bool Unavailable { get; init; } = false;
        public required string CreateDateShamsi { get; init; }
        public required string LastUpdateDateShamsi { get; init; }
    }
}
