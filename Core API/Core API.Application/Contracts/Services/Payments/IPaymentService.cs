using Core_API.Application.Common.Models;
using Core_API.Application.Common.Results;
using Core_API.Application.DTOs.Payments.Requests;
using Core_API.Application.DTOs.Payments.Responses;

namespace Core_API.Application.Contracts.Services.Payments
{
    public interface IPaymentService
    {
        Task<OperationResult<PaymentResponseDto>> GetPaymentByIdAsync(int id);
        Task<OperationResult<PaginatedResult<PaymentResponseDto>>> GetPagedPaymentsAsync(PaymentFilterDto filter, OperationContext context);
        Task<OperationResult<PaymentResponseDto>> CreatePaymentAsync(CreatePaymentDto dto, OperationContext context);
        Task<OperationResult<PaymentResponseDto>> UpdatePaymentAsync(UpdatePaymentDto dto, OperationContext context);
        Task<OperationResult<bool>> DeletePaymentAsync(int id, OperationContext context);
        Task<OperationResult<PaymentResponseDto>> ProcessPaymentAsync(ProcessPaymentDto dto, OperationContext context);
        Task<OperationResult<PaymentResponseDto>> RefundPaymentAsync(RefundPaymentDto dto, OperationContext context);
        Task<OperationResult<PaymentStatsDto>> GetPaymentStatsAsync(OperationContext context);
        Task<OperationResult<decimal>> GetCustomerOutstandingBalanceAsync(int customerId, OperationContext context);
    }
}