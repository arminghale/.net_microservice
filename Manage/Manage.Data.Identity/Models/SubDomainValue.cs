using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Manage.Data.Identity.Models
{
    public class SubDomainValue
    {
        public SubDomainValue()
        {

        }
        [Key]
        public int id { get; set; }

        [ForeignKey("Parent")]
        public int ParentId { get; set; }

        [ForeignKey("Child")]
        public int ChildId { get; set; }
        public virtual DomainValue? Parent { get; set; }
        public virtual DomainValue? Child { get; set; }
    }
}
