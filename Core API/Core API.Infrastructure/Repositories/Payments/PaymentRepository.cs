namespace Core_API.Infrastructure.Repositories.Payment
{
    //public class PaymentRepository(CoreInvoiceDbContext context) : GenericRepository<Payment>(context), IPaymentRepository
    //{
    //    private readonly CoreInvoiceDbContext _context = context;

    //    public async Task<Payment> GetByPaymentNumberAsync(string paymentNumber)
    //    {
    //        return await _context.Payments
    //            .FirstOrDefaultAsync(p => p.PaymentNumber == paymentNumber);
    //    }

    //    public async Task<IEnumerable<Payment>> GetByInvoiceIdAsync(int invoiceId)
    //    {
    //        return await _context.InvoicePayments
    //            .Where(p => p.InvoiceId == invoiceId && !p.IsDeleted)
    //            .OrderByDescending(p => p.PaymentDate)
    //            .ToListAsync();
    //    }

    //    public async Task<IEnumerable<Payment>> GetByCustomerIdAsync(int customerId)
    //    {
    //        return await _context.Payments
    //            .Where(p => p.CustomerId == customerId && !p.IsDeleted)
    //            .OrderByDescending(p => p.PaymentDate)
    //            .ToListAsync();
    //    }

    //    public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int? companyId = null)
    //    {
    //        var query = _context.Payments
    //            .Where(p => p.PaymentDate >= fromDate && p.PaymentDate <= toDate && !p.IsDeleted);

    //        if (companyId.HasValue)
    //        {
    //            query = query.Where(p => p.CompanyId == companyId.Value);
    //        }

    //        return await query
    //            .OrderByDescending(p => p.PaymentDate)
    //            .ToListAsync();
    //    }

    //    public async Task<decimal> GetTotalPaymentsForInvoiceAsync(int invoiceId)
    //    {
    //        return await _context.Payments
    //            .Where(p => p.InvoiceId == invoiceId && !p.IsDeleted && p.PaymentStatus == PaymentStatus.Paid)
    //            .SumAsync(p => p.Amount);
    //    }

    //    public async Task<decimal> GetTotalPaymentsForCustomerAsync(int customerId)
    //    {
    //        return await _context.Payments
    //            .Where(p => p.CustomerId == customerId && !p.IsDeleted && p.PaymentStatus == PaymentStatus.Paid)
    //            .SumAsync(p => p.Amount);
    //    }

    //    public async Task<Payment> GetPaymentWithDetailsAsync(int id)
    //    {
    //        return await _context.Payments
    //            .Include(p => p.Invoice)
    //            .Include(p => p.Customer)
    //            .Include(p => p.Company)
    //            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    //    }

    //    public async Task<IEnumerable<Payment>> GetRecentPaymentsAsync(int count, int? companyId = null)
    //    {
    //        var query = _context.Payments
    //            .Include(p => p.Invoice)
    //            .Include(p => p.Customer)
    //            .Where(p => !p.IsDeleted);

    //        if (companyId.HasValue)
    //        {
    //            query = query.Where(p => p.CompanyId == companyId.Value);
    //        }

    //        return await query
    //            .OrderByDescending(p => p.PaymentDate)
    //            .Take(count)
    //            .ToListAsync();
    //    }

    //    public async Task<Dictionary<string, decimal>> GetPaymentMethodBreakdownAsync(int? companyId = null)
    //    {
    //        var query = _context.Payments
    //            .Where(p => !p.IsDeleted && p.PaymentStatus == PaymentStatus.Paid);

    //        if (companyId.HasValue)
    //        {
    //            query = query.Where(p => p.CompanyId == companyId.Value);
    //        }

    //        return await query
    //            .GroupBy(p => p.PaymentMethod)
    //            .Select(g => new { PaymentMethod = g.Key, Total = g.Sum(p => p.Amount) })
    //            .ToDictionaryAsync(g => g.PaymentMethod, g => g.Total);
    //    }

    //    public async Task<Dictionary<string, int>> GetStatusBreakdownAsync(int? companyId = null)
    //    {
    //        var query = _context.Payments
    //            .Where(p => !p.IsDeleted);

    //        if (companyId.HasValue)
    //        {
    //            query = query.Where(p => p.CompanyId == companyId.Value);
    //        }

    //        return await query
    //            .GroupBy(p => p.PaymentStatus.ToString())
    //            .Select(g => new { Status = g.Key, Count = g.Count() })
    //            .ToDictionaryAsync(g => g.Status, g => g.Count);
    //    }

    //    public async Task<string> GenerateNextPaymentNumberAsync(int companyId)
    //    {
    //        var currentYear = DateTime.UtcNow.Year;
    //        var prefix = "PAY";

    //        var lastPayment = await _context.Payments
    //            .Where(p => p.CompanyId == companyId && p.PaymentNumber.StartsWith($"{prefix}-{currentYear}"))
    //            .OrderByDescending(p => p.PaymentNumber)
    //            .FirstOrDefaultAsync();

    //        int nextNumber = 1;
    //        if (lastPayment != null)
    //        {
    //            var parts = lastPayment.PaymentNumber.Split('-');
    //            if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
    //            {
    //                nextNumber = lastNumber + 1;
    //            }
    //        }

    //        return $"{prefix}-{currentYear}-{nextNumber:D4}";
    //    }
    //}
}
