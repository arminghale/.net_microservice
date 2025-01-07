using Manage.Data.Management.DTO.General;
using Manage.Data.Public;
using System.Diagnostics.CodeAnalysis;

namespace Manage.Data.Management.DTO.DomainValue
{

    public record DomainValueList
    {
        [SetsRequiredMembers]
        public DomainValueList(Models.DomainValue domainValue)
        {
            Id = domainValue.Id;
            DomainId = domainValue.DomainId;
            Domain = domainValue.Domain?.Title;
            ParentId = domainValue.AsChild?.FirstOrDefault()?.ParentId;
            Parent = domainValue.AsChild?.FirstOrDefault()?.Parent?.Value;
            Unavailable = domainValue.Delete;

            CreateDateShamsi = DateDifference.MiladiToShamsi(domainValue.CreateDate);
            LastUpdateDateShamsi = DateDifference.MiladiToShamsi(domainValue.LastUpdateDate);

            if (domainValue.Value.StartsWith("file:"))
            {
                ValueFormatted = domainValue.Value.Split("file:")[1];
            }
            else if (domainValue.Value.StartsWith("location:"))
            {
                ValueFormatted = new LocationFormat(domainValue.Value.Split("location:")[1].Split('|')[0], domainValue.Value.Split("location:")[1].Split('|')[1]);
            }
            else
            {
                ValueFormatted = domainValue.Value;
            }

            if (domainValue.Value.StartsWith("file:"))
            {
                IsFile = true;
            }
            else
            {
                IsFile = false;
            }
            
            if (Uri.IsWellFormedUriString(domainValue.Value, UriKind.Absolute))
            {
                IsLink = true;
            }
            else
            {
                IsLink = false;
            }

            if (domainValue.Value.StartsWith("location:"))
            {
                IsLocation = true;
            }
            else
            {
                IsLocation = false;
            }


        }
        public int Id { get; init; }
        public int DomainId { get; init; }
        public string? Domain { get; init; }
        public int? ParentId { get; set; }
        public string? Parent { get; set; }
        public required string CreateDateShamsi { get; init; }
        public required string LastUpdateDateShamsi { get; init; }
        public required object ValueFormatted { get; init; }
        public bool IsLink { get; init; }
        public bool IsFile { get; init; }
        public bool IsLocation { get; init; }
        public bool Unavailable { get; init; } = false;
    }

    public record DomainValueOne
    {
        public DomainValueOne(Models.DomainValue domainValue)
        {
            DomainValue = new DomainValueList(domainValue);
            if (domainValue.AsChild.FirstOrDefault() != null)
            {
                Parent = new DomainValueList(domainValue.AsChild.FirstOrDefault().Parent);
            }
            Childs = domainValue.AsParent.Select(w => new DomainValueList(w.Child));
        }
        public DomainValueList DomainValue { get; init; }
        public DomainValueList? Parent { get; init; } = null;
        public IEnumerable<DomainValueList>? Childs { get; init; }
    }
}
