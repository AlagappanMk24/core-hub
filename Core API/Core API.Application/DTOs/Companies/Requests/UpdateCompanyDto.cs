using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Companies.Requests
{
    public class UpdateCompanyDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public Domain.ValueObjects.Email? Email { get; set; }

        [StringLength(20)]
        [Phone]
        public string? PhoneNumber { get; set; }

        public CompanyAddressDto? Address { get; set; }
    }
}
