using System.Diagnostics.CodeAnalysis;
using Manage.Data.Public;

namespace Manage.Data.Identity.DTO.Tenant
{

    public record TenantList
    {
        [SetsRequiredMembers]
        public TenantList(Models.Tenant tenant)
        {
            Id = tenant.Id;
            Title = tenant.Title;
            Unavailable = tenant.Delete;
            CreateDateShamsi = DateDifference.MiladiToShamsi(tenant.CreateDate);
            LastUpdateDateShamsi = DateDifference.MiladiToShamsi(tenant.LastUpdateDate);
        }
        public int Id { get; init; }
        public required string Title { get; init; }
        public bool Unavailable { get; init; } = false;
        public required string CreateDateShamsi { get; init; }
        public required string LastUpdateDateShamsi { get; init; }
    }
    public record TenantOne
    {
        [SetsRequiredMembers]
        public TenantOne(Models.Tenant tenant)
        {
            Id = tenant.Id;
            Title = tenant.Title;
            Unavailable = tenant.Delete;
            RegisterRoles = tenant.RegisterRoles.Split(',').Select(w => int.Parse(w)).ToArray();
            AdminRoles = tenant.AdminRoles.Split(',').Select(w => int.Parse(w)).ToArray();
            CreateDateShamsi = DateDifference.MiladiToShamsi(tenant.CreateDate);
            LastUpdateDateShamsi = DateDifference.MiladiToShamsi(tenant.LastUpdateDate);
        }
        public int Id { get; init; }
        public required string Title { get; init; }
        public required int[] RegisterRoles { get; init; }
        public required int[] AdminRoles { get; init; }
        public bool Unavailable { get; init; } = false;
        public required string CreateDateShamsi { get; init; }
        public required string LastUpdateDateShamsi { get; init; }
    }

}
