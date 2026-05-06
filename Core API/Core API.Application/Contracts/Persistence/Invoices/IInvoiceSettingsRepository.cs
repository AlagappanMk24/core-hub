using Core_API.Domain.Entities.Invoices;

namespace Core_API.Application.Contracts.Persistence.Invoice
{
    public interface IInvoiceSettingsRepository
    {
        /// <summary>
        /// Gets invoice settings by company ID
        /// </summary>
        Task<InvoiceSettings> GetByCompanyIdAsync(int companyId);

        /// <summary>
        /// Adds new invoice settings
        /// </summary>
        Task AddAsync(InvoiceSettings settings);

        /// <summary>
        /// Updates existing invoice settings
        /// </summary>
        void Update(InvoiceSettings settings);

        /// <summary>
        /// Saves or updates invoice settings (upsert)
        /// </summary>
        Task SaveAsync(InvoiceSettings settings);

        /// <summary>
        /// Gets the next available invoice number for a company
        /// </summary>
        Task<string> GetNextInvoiceNumberAsync(int companyId);
    }
}