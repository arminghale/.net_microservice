using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manage.Data.Management.Models
{
    public class DomainValue
    {
        public DomainValue()
        {
        }
        [Key]
        public int Id { get; set; }
        [ForeignKey("Domain")]
        public int DomainId { get; set; }
        public required string Value { get; set; }  //file:bucket/object   location:12.52115(longitude)|16.2585(latitude)   value
        public bool Delete { get; set; } = false;

        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime LastUpdateDate { get; set; } = DateTime.Now;

        public virtual Domain? Domain { get; set; }
        public virtual IEnumerable<SubDomainValue> AsParent { get; set; } = Enumerable.Empty<SubDomainValue>();
        public virtual IEnumerable<SubDomainValue> AsChild { get; set; } = Enumerable.Empty<SubDomainValue>();

    }
}
