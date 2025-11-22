namespace Core_API.Application.DTOs.Customer.Request;

public class CustomerFilterRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? Status { get; set; } // All, Active, Inactive

    public bool IsValid()
    {
        if (PageNumber < 1 || PageSize < 1)
        {
            return false;
        }
        if (!string.IsNullOrEmpty(Status) && Status != "All" && Status != "Active" && Status != "Inactive")
        {
            return false;
        }
        return true;
    }
}