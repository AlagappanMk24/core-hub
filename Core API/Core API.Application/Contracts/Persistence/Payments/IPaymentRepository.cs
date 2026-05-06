using Core_API.Domain.Entities.Payments;

namespace Core_API.Application.Contracts.Persistence.Payments
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<Payment> GetByPaymentNumberAsync(string paymentNumber);
        Task<IEnumerable<Payment>> GetByInvoiceIdAsync(int invoiceId);
        Task<IEnumerable<Payment>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int? companyId = null);
        Task<decimal> GetTotalPaymentsForInvoiceAsync(int invoiceId);
        Task<decimal> GetTotalPaymentsForCustomerAsync(int customerId);
        Task<Payment> GetPaymentWithDetailsAsync(int id);
        Task<IEnumerable<Payment>> GetRecentPaymentsAsync(int count, int? companyId = null);
        Task<Dictionary<string, decimal>> GetPaymentMethodBreakdownAsync(int? companyId = null);
        Task<Dictionary<string, int>> GetStatusBreakdownAsync(int? companyId = null);
        Task<string> GenerateNextPaymentNumberAsync(int companyId);
    }
}