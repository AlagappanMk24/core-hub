using Core_API.Domain.Entities;
using Core_API.Domain.ValueObjects;

namespace Core_API.Application.DTOs.Companies.Responses
{
    public class CompanyResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? TaxId { get; set; }
        public Address? Address { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsDeleted { get; set; }
        public string CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
