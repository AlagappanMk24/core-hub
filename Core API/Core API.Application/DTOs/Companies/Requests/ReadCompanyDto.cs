using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Companies.Requests
{
    public class ReadCompanyDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public CompanyAddressDto? Address { get; set; }
    }
    public class CompanyAddressDto
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