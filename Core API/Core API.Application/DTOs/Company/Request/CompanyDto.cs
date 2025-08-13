using Core_API.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Company.Request
{
    public class CompanyCreateDto
    {
        [Required]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [StringLength(100, ErrorMessage = "Tax ID cannot exceed 100 characters.")]
        public string? TaxId { get; set; }

        public Address? Address { get; set; }

        [EmailAddress]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
        public string? Email { get; set; }

        [Phone]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        public string? PhoneNumber { get; set; }
    }

    public class CompanyUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(20)]
        [Phone]
        public string? PhoneNumber { get; set; }

        public AddressDto? Address { get; set; }
    }
    public class CompanyReadDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public AddressDto? Address { get; set; }
    }
    public class AddressDto
    {
        [StringLength(100)]
        public string? Address1 { get; set; }

        [StringLength(100)]
        public string? Address2 { get; set; }

        [StringLength(50)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        [StringLength(50)]
        public string? Country { get; set; }

        [StringLength(20)]
        public string? ZipCode { get; set; }
    }
}
