using System.Diagnostics.CodeAnalysis;
using Manage.Data.Management.DTO.ActionGroup;
using Manage.Data.Public;

namespace Manage.Data.Management.DTO.Service
{

    public record ServiceList
    {
        [SetsRequiredMembers]
        public ServiceList(Models.Service service)
        {
            Id = service.Id;
            Title = service.Title;
            Unavailable = service.Delete;
            CreateDateShamsi = DateDifference.MiladiToShamsi(service.CreateDate);
            LastUpdateDateShamsi = DateDifference.MiladiToShamsi(service.LastUpdateDate);
        }
        public int Id { get; init; }
        public required string Title { get; init; }
        public bool Unavailable { get; init; } = false;
        public required string CreateDateShamsi { get; init; }
        public required string LastUpdateDateShamsi { get; init; }
    }
    public record ServiceOne
    {
        public ServiceOne(Models.Service service)
        {
            Service = new ServiceList(service);
            ActionGroups = service.ActionGroups.Select(w => new ActionGroupList(w));
        }
        public ServiceList Service { get; init; }
        public IEnumerable<ActionGroupList> ActionGroups { get; init; }
    }
}
