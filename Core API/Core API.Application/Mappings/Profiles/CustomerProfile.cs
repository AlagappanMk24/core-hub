using AutoMapper;
using Core_API.Application.DTOs.Customer.Request;
using Core_API.Application.DTOs.Customer.Response;
using Core_API.Application.Features.Customers.Commands.CreateCustomer;
using Core_API.Application.Features.Customers.Commands.UpdateCustomer;
using Core_API.Domain.Entities.Customers;
using Core_API.Domain.Entities.Invoices;
using Core_API.Domain.Enums;

namespace Core_API.Application.Mappings.Profiles
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            #region Customer to Response DTO
            CreateMap<Customer, CustomerResponseDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber.Value))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.CustomerGroupName, opt => opt.MapFrom(src => src.CustomerGroup != null ? src.CustomerGroup.Name : null))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : null))
                .ForMember(dest => dest.AddressLine1, opt => opt.MapFrom(src => src.Address.AddressLine1))
                .ForMember(dest => dest.AddressLine2, opt => opt.MapFrom(src => src.Address.AddressLine2))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Address.City))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.Address.State))
                .ForMember(dest => dest.CountryCode, opt => opt.MapFrom(src => src.Address.CountryCode))
                .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Address.CountryName))
                .ForMember(dest => dest.ZipCode, opt => opt.MapFrom(src => src.Address.ZipCode));
            #endregion

            #region Create DTO to Customer
            CreateMap<CreateCustomerDto, Customer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Address, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerGroup, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => CustomerStatus.Active))
                .ForMember(dest => dest.ActiveSince, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.LastPurchaseDate, opt => opt.Ignore())
                .ForMember(dest => dest.TotalPurchases, opt => opt.Ignore())
                .ForMember(dest => dest.AveragePaymentDays, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.RecurringInvoices, opt => opt.Ignore())
                .ForMember(dest => dest.Notes, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore())
                .ForMember(dest => dest.Contacts, opt => opt.Ignore())
                .ForMember(dest => dest.TaxId, opt => opt.MapFrom(src => src.TaxId))
                .ForMember(dest => dest.Website, opt => opt.MapFrom(src => src.Website))
                .ForMember(dest => dest.CreditLimit, opt => opt.MapFrom(src => src.CreditLimit))
                .ForMember(dest => dest.DefaultPaymentTerms, opt => opt.MapFrom(src => src.DefaultPaymentTerms))
                .ForMember(dest => dest.DefaultCurrency, opt => opt.MapFrom(src => src.DefaultCurrency))
                .ForMember(dest => dest.CustomerGroupId, opt => opt.MapFrom(src => src.CustomerGroupId));
            #endregion

            #region Update DTO to Customer
            CreateMap<UpdateCustomerDto, Customer>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Address, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerGroup, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.ActiveSince, opt => opt.Ignore())
                .ForMember(dest => dest.LastPurchaseDate, opt => opt.Ignore())
                .ForMember(dest => dest.TotalPurchases, opt => opt.Ignore())
                .ForMember(dest => dest.AveragePaymentDays, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.RecurringInvoices, opt => opt.Ignore())
                .ForMember(dest => dest.Notes, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore())
                .ForMember(dest => dest.Contacts, opt => opt.Ignore())
                .ForMember(dest => dest.TaxId, opt => opt.MapFrom(src => src.TaxId))
                .ForMember(dest => dest.Website, opt => opt.MapFrom(src => src.Website))
                .ForMember(dest => dest.CreditLimit, opt => opt.MapFrom(src => src.CreditLimit))
                .ForMember(dest => dest.DefaultPaymentTerms, opt => opt.MapFrom(src => src.DefaultPaymentTerms))
                .ForMember(dest => dest.DefaultCurrency, opt => opt.MapFrom(src => src.DefaultCurrency))
                .ForMember(dest => dest.CustomerGroupId, opt => opt.MapFrom(src => src.CustomerGroupId));
            #endregion

            #region Invoice to CustomerInvoiceDto
            CreateMap<Invoice, CustomerInvoiceDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.InvoiceNumber))
                .ForMember(dest => dest.IssueDate, opt => opt.MapFrom(src => src.IssueDate))
                .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.InvoiceStatus.ToString()))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.PaymentStatus.ToString()));
            #endregion

            #region Payment to CustomerPaymentDto
            CreateMap<InvoicePayment, CustomerPaymentDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.PaymentNumber, opt => opt.MapFrom(src => src.PaymentNumber))
                .ForMember(dest => dest.InvoiceId, opt => opt.MapFrom(src => src.InvoiceId))
                .ForMember(dest => dest.InvoiceNumber, opt => opt.MapFrom(src => src.Invoice != null ? src.Invoice.InvoiceNumber : "N/A"))
                .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Amount))
                .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => src.PaymentDate))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.PaymentMethod))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.PaymentStatus.ToString()))
                .ForMember(dest => dest.IsOnTime, opt => opt.MapFrom(src => src.PaymentDate <= src.Invoice!.DueDate));
            #endregion

            #region Create Command to Entity
            CreateMap<CreateCustomerCommand, Customer>()
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Address, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => CustomerStatus.Active))
                .ForMember(dest => dest.ActiveSince, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore())
                .ForMember(dest => dest.RecurringInvoices, opt => opt.Ignore())
                .ForMember(dest => dest.Notes, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore())
                .ForMember(dest => dest.Contacts, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerGroup, opt => opt.Ignore())
                .ForMember(dest => dest.LastPurchaseDate, opt => opt.Ignore())
                .ForMember(dest => dest.TotalPurchases, opt => opt.Ignore())
                .ForMember(dest => dest.AveragePaymentDays, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TaxId, opt => opt.MapFrom(src => src.TaxId))
                .ForMember(dest => dest.Website, opt => opt.MapFrom(src => src.Website))
                .ForMember(dest => dest.CreditLimit, opt => opt.MapFrom(src => src.CreditLimit))
                .ForMember(dest => dest.DefaultPaymentTerms, opt => opt.MapFrom(src => src.DefaultPaymentTerms))
                .ForMember(dest => dest.DefaultCurrency, opt => opt.MapFrom(src => src.DefaultCurrency))
                .ForMember(dest => dest.CustomerGroupId, opt => opt.MapFrom(src => src.CustomerGroupId));
            #endregion

            #region Update Command to Entity
            CreateMap<UpdateCustomerCommand, Customer>()
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
                .ForMember(dest => dest.Address, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Invoices, opt => opt.Ignore())
                .ForMember(dest => dest.RecurringInvoices, opt => opt.Ignore())
                .ForMember(dest => dest.Notes, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore())
                .ForMember(dest => dest.Contacts, opt => opt.Ignore())
                .ForMember(dest => dest.Company, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.ActiveSince, opt => opt.Ignore())
                .ForMember(dest => dest.TotalPurchases, opt => opt.Ignore())
                .ForMember(dest => dest.AveragePaymentDays, opt => opt.Ignore())
                .ForMember(dest => dest.LastPurchaseDate, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerGroup, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TaxId, opt => opt.MapFrom(src => src.TaxId))
                .ForMember(dest => dest.Website, opt => opt.MapFrom(src => src.Website))
                .ForMember(dest => dest.CreditLimit, opt => opt.MapFrom(src => src.CreditLimit))
                .ForMember(dest => dest.DefaultPaymentTerms, opt => opt.MapFrom(src => src.DefaultPaymentTerms))
                .ForMember(dest => dest.DefaultCurrency, opt => opt.MapFrom(src => src.DefaultCurrency))
                .ForMember(dest => dest.CustomerGroupId, opt => opt.MapFrom(src => src.CustomerGroupId));
            #endregion
        }
    }
}