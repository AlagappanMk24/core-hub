using Core_API.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Companies.Requests
{
    public class CreateCompanyDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [StringLength(100, ErrorMessage = "Tax ID cannot exceed 100 characters.")]
        public string? TaxId { get; set; }

        public Address? Address { get; set; }

        [EmailAddress]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public Domain.ValueObjects.Email? Email { get; set; }

        [Phone]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        public PhoneNumber? PhoneNumber { get; set; }
    }
}
