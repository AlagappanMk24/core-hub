using Core_API.Domain.Entities.Common;

namespace Core_API.Domain.Entities
{
    public class EmailSettings : BaseEntity
    {
        public int CompanyId { get; set; }
        public string FromEmail { get; set; }
    }
}