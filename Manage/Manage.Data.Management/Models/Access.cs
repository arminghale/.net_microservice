namespace Manage.Data.Management.Models
{
    public class Access
    {
        public int UserId { get; set; }
        public int ActionId { get; set; }
        public int ActionGroupId { get; set; }
        public int ServiceId { get; set; }
        public int TenantId { get; set; }
        public int type { get; set; }
        public virtual Action? Action { get; set; }
    }
}
