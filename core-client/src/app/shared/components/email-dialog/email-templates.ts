export interface EmailTemplate {
  subject: string;
  message: string;
}

// Customer-focused templates
export const CustomerEmailTemplates: Record<string, EmailTemplate> = {
  custom: {
    subject: 'Message from {{companyName}}',
    message: `Dear {{customerName}},

We hope this message finds you well.

Best regards,
{{companyName}} Team`,
  },
  statement: {
    subject: 'Customer Statement - {{customerName}}',
    message: `Dear {{customerName}},

Please find attached your latest customer statement for your reference.

This statement includes all your invoices and payment history up to {{date}}.

If you have any questions or notice any discrepancies, please don't hesitate to contact us.

Best regards,
{{companyName}} Team`,
  },
  payment_reminder: {
    subject: 'Payment Reminder: Payment Due Soon',
    message: `Dear {{customerName}},

This is a friendly reminder that payment is due soon.

Please arrange payment at your earliest convenience.

Payment Details:
Bank: {{bankName}}
Account: {{accountNumber}}

If you have already made the payment, please disregard this message.

Best regards,
{{companyName}} Team`,
  },
  thank_you: {
    subject: 'Thank You for Your Business',
    message: `Dear {{customerName}},

Thank you for your continued business partnership.

We truly appreciate having you as a valued customer. If there's anything we can assist you with, please don't hesitate to reach out.

Best regards,
{{companyName}} Team`,
  },
  welcome: {
    subject: 'Welcome to {{companyName}}!',
    message: `Dear {{customerName}},

Welcome to {{companyName}}! We're excited to have you as a customer.

Here's what you can expect:
• Invoices will be sent to this email address
• You can view your invoice history online
• 24/7 customer support

If you have any questions, please contact our support team.

Best regards,
{{companyName}} Team`,
  },
};

// Invoice-focused templates
export const InvoiceEmailTemplates: Record<string, EmailTemplate> = {
  custom: {
    subject: 'Invoice #{{invoiceNumber}} from {{companyName}}',
    message: `Dear {{customerName}},

Please find attached invoice #{{invoiceNumber}} for your reference.

Best regards,
{{companyName}} Team`,
  },
  new_invoice: {
    subject: 'New Invoice #{{invoiceNumber}} from {{companyName}}',
    message: `Dear {{customerName}},

We have issued a new invoice #{{invoiceNumber}} for your recent purchase.

Invoice Details:
• Invoice Number: {{invoiceNumber}}
• Amount Due: {{amount}}
• Due Date: {{dueDate}}

Please find attached the invoice PDF for your records.

Thank you for your business!

Best regards,
{{companyName}} Team`,
  },
  payment_reminder: {
    subject: 'Payment Reminder: Invoice #{{invoiceNumber}} Due Soon',
    message: `Dear {{customerName}},

This is a friendly reminder that payment for invoice #{{invoiceNumber}} is due soon.

Invoice Details:
• Invoice Number: {{invoiceNumber}}
• Amount Due: {{amount}}
• Due Date: {{dueDate}}

Please arrange payment at your earliest convenience.

If you have already made the payment, please disregard this message.

Best regards,
{{companyName}} Team`,
  },
  overdue_notice: {
    subject: 'Overdue Notice: Invoice #{{invoiceNumber}}',
    message: `Dear {{customerName}},

We noticed that payment for invoice #{{invoiceNumber}} is now overdue.

Invoice Details:
• Invoice Number: {{invoiceNumber}}
• Amount Due: {{amount}}
• Original Due Date: {{dueDate}}

To avoid any service interruption, please process the payment as soon as possible.

If you have already sent the payment, please let us know the transaction details.

Best regards,
{{companyName}} Team`,
  },
  payment_received: {
    subject: 'Payment Received for Invoice #{{invoiceNumber}}',
    message: `Dear {{customerName}},

Thank you for your payment of {{amount}} for invoice #{{invoiceNumber}}.

We truly appreciate your business and timely payment.

If you need anything else, please don't hesitate to reach out.

Best regards,
{{companyName}} Team`,
  },
  invoice_copy: {
    subject: 'Copy of Invoice #{{invoiceNumber}}',
    message: `Dear {{customerName}},

As requested, please find attached a copy of invoice #{{invoiceNumber}}.

Invoice Details:
• Invoice Number: {{invoiceNumber}}
• Amount: {{amount}}
• Issue Date: {{date}}

Thank you for your business!

Best regards,
{{companyName}} Team`,
  },
};

// Helper functions
export function getCustomerEmailTemplate(key: string): EmailTemplate {
  return CustomerEmailTemplates[key] || CustomerEmailTemplates['custom'];
}

export function getInvoiceEmailTemplate(key: string): EmailTemplate {
  return InvoiceEmailTemplates[key] || InvoiceEmailTemplates['custom'];
}
