using System.ComponentModel.DataAnnotations;

namespace Core_API.Application.DTOs.Customer.Request;

public class CustomerCreateDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Address1 is required")]
    [StringLength(200, ErrorMessage = "Address1 cannot exceed 200 characters")]
    public string Address1 { get; set; }

    [StringLength(200, ErrorMessage = "Address2 cannot exceed 200 characters")]
    public string Address2 { get; set; }

    [Required(ErrorMessage = "City is required")]
    [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
    public string City { get; set; }

    [StringLength(100, ErrorMessage = "State cannot exceed 100 characters")]
    public string State { get; set; }

    [Required(ErrorMessage = "Country is required")]
    [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
    public string Country { get; set; }

    [Required(ErrorMessage = "Zip code is required")]
    [StringLength(20, ErrorMessage = "Zip code cannot exceed 20 characters")]
    public string ZipCode { get; set; }
}
