using Core_API.Domain.Common;

namespace Core_API.Domain.Entities.Settings
{
    public class EmailSettings : BaseEntity
    {
        public int CompanyId { get; set; }
        public string FromEmail { get; set; }
    }
}