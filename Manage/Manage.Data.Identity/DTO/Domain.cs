using System.Diagnostics.CodeAnalysis;
using Manage.Data.Identity.DTO.DomainValue;
using Manage.Data.Public;

namespace Manage.Data.Identity.DTO.Domain
{

    public record DomainList
    {
        [SetsRequiredMembers]
        public DomainList(Models.Domain domain)
        {
            Id = domain.Id;
            Title = domain.Title;
            Unavailable = domain.Delete;
            CreateDateShamsi = DateDifference.MiladiToShamsi(domain.CreateDate);
            LastUpdateDateShamsi = DateDifference.MiladiToShamsi(domain.LastUpdateDate);
        }
        public int Id { get; init; }
        public required string Title { get; init; }
        public bool Unavailable { get; init; } = false;
        public required string CreateDateShamsi { get; init; }
        public required string LastUpdateDateShamsi { get; init; }
    }
    public record DomainOne
    {
        public DomainOne(Models.Domain domain)
        {
            Domain = new DomainList(domain);
            DomainValues = domain.DomainValues.Select(w => new DomainValueList(w));
        }
        public DomainList Domain { get; init; }
        public IEnumerable<DomainValueList> DomainValues { get; init; }
    }
}
