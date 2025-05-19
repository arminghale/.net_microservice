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
        public virtual ICollection<SubDomainValue> AsParent { get; set; } = new List<SubDomainValue>();
        public virtual ICollection<SubDomainValue> AsChild { get; set; } = new List<SubDomainValue>();

    }
}
