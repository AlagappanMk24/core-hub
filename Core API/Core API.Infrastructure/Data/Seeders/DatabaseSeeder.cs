using Core_API.Domain.Entities;
using Core_API.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Seeders
{
    public static class DatabaseSeeder
    {
        //public static void SeedData(ModelBuilder modelBuilder)
        //{
        //    // Seed Customers
        //    modelBuilder.Entity<Customer>().HasData(
        //        new Customer { Id = 1, Name = "John Doe", Email = "john.doe@example.com", PhoneNumber = "555-0101", CompanyId = 1 },
        //        new Customer { Id = 2, Name = "Jane Smith", Email = "jane.smith@example.com", PhoneNumber = "555-0102", CompanyId = 1 },
        //        new Customer { Id = 3, Name = "Acme Corp", Email = "billing@acmecorp.com", PhoneNumber = "555-0103", CompanyId = 1 },
        //        new Customer { Id = 4, Name = "Sarah Johnson", Email = "sarah.j@example.com", PhoneNumber = "555-0104", CompanyId = 2 },
        //        new Customer { Id = 5, Name = "TechTrend Ltd", Email = "accounts@techtrend.com", PhoneNumber = "555-0105", CompanyId = 2 },
        //        new Customer { Id = 6, Name = "Michael Brown", Email = "michael.brown@example.com", PhoneNumber = "555-0106", CompanyId = 3 },
        //        new Customer { Id = 7, Name = "Global Traders", Email = "finance@globaltraders.com", PhoneNumber = "555-0107", CompanyId = 3 },
        //        new Customer { Id = 8, Name = "Emily Davis", Email = "emily.davis@example.com", PhoneNumber = "555-0108", CompanyId = 4 },
        //        new Customer { Id = 9, Name = "Innovate Designs", Email = "billing@innovatedesigns.com", PhoneNumber = "555-0109", CompanyId = 4 },
        //        new Customer { Id = 10, Name = "Robert Wilson", Email = "robert.w@example.com", PhoneNumber = "555-0110", CompanyId = 5 },
        //        new Customer { Id = 11, Name = "Bright Solutions", Email = "accounts@brightsolutions.com", PhoneNumber = "555-0111", CompanyId = 5 },
        //        new Customer { Id = 12, Name = "Laura Martinez", Email = "laura.m@example.com", PhoneNumber = "555-0112", CompanyId = 6 },
        //        new Customer { Id = 13, Name = "NexGen Innovations", Email = "finance@nexgen.com", PhoneNumber = "555-0113", CompanyId = 6 }
        //    );

        //    modelBuilder.Entity<Customer>()
        //        .OwnsOne(c => c.Address)
        //        .HasData(
        //            new { CustomerId = 1, Address1 = "123 Maple St", Address2 = "Apt 4B", City = "Springfield", State = "IL", Country = "USA", ZipCode = "62701" },
        //            new { CustomerId = 2, Address1 = "456 Oak Ave", Address2 = "Suite 201", City = "London", State = "Greater London", Country = "UK", ZipCode = "SW1A 1AA" },
        //            new { CustomerId = 3, Address1 = "789 Pine Rd", Address2 = "", City = "New York", State = "NY", Country = "USA", ZipCode = "10001" },
        //            new { CustomerId = 4, Address1 = "101 Elm St", Address2 = "", City = "Boston", State = "MA", Country = "USA", ZipCode = "02108" },
        //            new { CustomerId = 5, Address1 = "202 Birch Ln", Address2 = "Floor 3", City = "Toronto", State = "ON", Country = "Canada", ZipCode = "M5V 2T7" },
        //            new { CustomerId = 6, Address1 = "303 Cedar Ave", Address2 = "", City = "Sydney", State = "NSW", Country = "Australia", ZipCode = "2000" },
        //            new { CustomerId = 7, Address1 = "404 Spruce St", Address2 = "Unit 5", City = "Singapore", State = "", Country = "Singapore", ZipCode = "069609" },
        //            new { CustomerId = 8, Address1 = "505 Walnut Rd", Address2 = "", City = "San Francisco", State = "CA", Country = "USA", ZipCode = "94103" },
        //            new { CustomerId = 9, Address1 = "606 Chestnut Dr", Address2 = "Suite 101", City = "Berlin", State = "", Country = "Germany", ZipCode = "10115" },
        //            new { CustomerId = 10, Address1 = "707 Sycamore Ave", Address2 = "", City = "Chicago", State = "IL", Country = "USA", ZipCode = "60614" },
        //            new { CustomerId = 11, Address1 = "808 Magnolia St", Address2 = "", City = "Paris", State = "", Country = "France", ZipCode = "75001" },
        //            new { CustomerId = 12, Address1 = "909 Laurel Rd", Address2 = "Apt 12", City = "Mumbai", State = "MH", Country = "India", ZipCode = "400001" },
        //            new { CustomerId = 13, Address1 = "1010 Willow Ln", Address2 = "", City = "Tokyo", State = "", Country = "Japan", ZipCode = "100-0001" }
        //        );
        //    // Seed Invoices
        //    modelBuilder.Entity<Invoice>().HasData(
        //        new Invoice
        //        {
        //            Id = 1,
        //            InvoiceNumber = "INV-001",
        //            PONumber = "PO-001",
        //            IssueDate = DateTime.UtcNow.AddDays(-30),
        //            PaymentDue = DateTime.UtcNow.AddDays(-15),
        //            Status = InvoiceStatus.Paid,
        //            InvoiceType = InvoiceType.Standard,
        //            CustomerId = 1,
        //            CompanyId = 1,
        //            Subtotal = 1000.00m,
        //            Tax = 100.00m,
        //            TotalAmount = 1100.00m,
        //            Notes = "Payment received on time.",
        //            PaymentMethod = "Bank Transfer",
        //            CreatedBy = "system",
        //            CreatedDate = DateTime.UtcNow.AddDays(-30)
        //        },
        //        new Invoice
        //        {
        //            Id = 2,
        //            InvoiceNumber = "INV-002",
        //            PONumber = "PO-002",
        //            IssueDate = DateTime.UtcNow.AddDays(-20),
        //            PaymentDue = DateTime.UtcNow.AddDays(-5),
        //            Status = InvoiceStatus.Overdue,
        //            InvoiceType = InvoiceType.Standard,
        //            CustomerId = 2,
        //            CompanyId = 1,
        //            Subtotal = 1500.00m,
        //            Tax = 150.00m,
        //            TotalAmount = 1650.00m,
        //            Notes = "Pending payment reminder sent.",
        //            PaymentMethod = "Credit Card",
        //            CreatedBy = "system",
        //            CreatedDate = DateTime.UtcNow.AddDays(-20)
        //        },
        //        new Invoice
        //        {
        //            Id = 3,
        //            InvoiceNumber = "INV-003",
        //            PONumber = "PO-003",
        //            IssueDate = DateTime.UtcNow.AddDays(-10),
        //            PaymentDue = DateTime.UtcNow.AddDays(5),
        //            Status = InvoiceStatus.Sent,
        //            InvoiceType = InvoiceType.Standard,
        //            CustomerId = 3,
        //            CompanyId = 1,
        //            Subtotal = 800.00m,
        //            Tax = 80.00m,
        //            TotalAmount = 880.00m,
        //            Notes = "Awaiting payment.",
        //            PaymentMethod = "PayPal",
        //            CreatedBy = "system",
        //            CreatedDate = DateTime.UtcNow.AddDays(-10)
        //        },
        //        new Invoice
        //        {
        //            Id = 4,
        //            InvoiceNumber = "INV-004",
        //            PONumber = "PO-004",
        //            IssueDate = DateTime.UtcNow.AddDays(-5),
        //            PaymentDue = DateTime.UtcNow.AddDays(10),
        //            Status = InvoiceStatus.Draft,
        //            InvoiceType = InvoiceType.Proforma,
        //            CustomerId = 4,
        //            CompanyId = 2,
        //            Subtotal = 2000.00m,
        //            Tax = 200.00m,
        //            TotalAmount = 2200.00m,
        //            Notes = "Draft for review.",
        //            PaymentMethod = null,
        //            CreatedBy = "system",
        //            CreatedDate = DateTime.UtcNow.AddDays(-5)
        //        },
        //        new Invoice
        //        {
        //            Id = 5,
        //            InvoiceNumber = "INV-005",
        //            PONumber = "PO-005",
        //            IssueDate = DateTime.UtcNow.AddDays(-25),
        //            PaymentDue = DateTime.UtcNow.AddDays(-10),
        //            Status = InvoiceStatus.PartiallyPaid,
        //            InvoiceType = InvoiceType.Standard,
        //            CustomerId = 5,
        //            CompanyId = 2,
        //            Subtotal = 3000.00m,
        //            Tax = 300.00m,
        //            TotalAmount = 3300.00m,
        //            Notes = "Partial payment of 1500 received.",
        //            PaymentMethod = "Bank Transfer",
        //            CreatedBy = "system",
        //            CreatedDate = DateTime.UtcNow.AddDays(-25)
        //        },
        //        new Invoice
        //        {
        //            Id = 6,
        //            InvoiceNumber = "INV-006",
        //            PONumber = "PO-006",
        //            IssueDate = DateTime.UtcNow.AddDays(-15),
        //            PaymentDue = DateTime.UtcNow,
        //            Status = InvoiceStatus.Overdue,
        //            InvoiceType = InvoiceType.Recurring,
        //            CustomerId = 6,
        //            CompanyId = 3,
        //            Subtotal = 1200.00m,
        //            Tax = 120.00m,
        //            TotalAmount = 1320.00m,
        //            Notes = "Monthly subscription invoice.",
        //            PaymentMethod = "Credit Card",
        //            CreatedBy = "system",
        //            CreatedDate = DateTime.UtcNow.AddDays(-15)
        //        },
        //        new Invoice
        //        {
        //            Id = 7,
        //            InvoiceNumber = "INV-007",
        //            PONumber = "PO-007",
        //            IssueDate = DateTime.UtcNow.AddDays(-8),
        //            PaymentDue = DateTime.UtcNow.AddDays(7),
        //            Status = InvoiceStatus.Sent,
        //            InvoiceType = InvoiceType.Standard,
        //            CustomerId = 7,
        //            CompanyId = 3,
        //            Subtotal = 1800.00m,
        //            Tax = 180.00m,
        //            TotalAmount = 1980.00m,
        //            Notes = "Awaiting payment confirmation.",
        //            PaymentMethod = "PayPal",
        //            CreatedBy = "system",
        //            CreatedDate = DateTime.UtcNow.AddDays(-8)
        //        },
        //        new Invoice
        //        {
        //            Id = 8,
        //            InvoiceNumber = "INV-008",
        //            PONumber = "PO-008",
        //            IssueDate = DateTime.UtcNow.AddDays(-3),
        //            PaymentDue = DateTime.UtcNow.AddDays(12),
        //            Status = InvoiceStatus.Draft,
        //            InvoiceType = InvoiceType.CreditNote,
        //            CustomerId = 8,
        //            CompanyId = 4,
        //            Subtotal = -500.00m,
        //            Tax = 0.00m,
        //            TotalAmount = -500.00m,
        //            Notes = "Credit note for returned items.",
        //            PaymentMethod = null,
        //            CreatedBy = "system",
        //            CreatedDate = DateTime.UtcNow.AddDays(-3)
        //        },
        //        new Invoice
        //        {
        //            Id = 9,
        //            InvoiceNumber = "INV-009",
        //            PONumber = "PO-009",
        //            IssueDate = DateTime.UtcNow.AddDays(-12),
        //            PaymentDue = DateTime.UtcNow.AddDays(3),
        //            Status = InvoiceStatus.Sent,
        //            InvoiceType = InvoiceType.Standard,
        //            CustomerId = 9,
        //            CompanyId = 4,
        //            Subtotal = 2500.00m,
        //            Tax = 250.00m,
        //            TotalAmount = 2750.00m,
        //            Notes = "Invoice for design services.",
        //            PaymentMethod = "Bank Transfer",
        //            CreatedBy = "system",
        //            CreatedDate = DateTime.UtcNow.AddDays(-12)
        //        },
        //        new Invoice
        //        {
        //            Id = 10,
        //            InvoiceNumber = "INV-010",
        //            PONumber = "PO-010",
        //            IssueDate = DateTime.UtcNow.AddDays(-18),
        //            PaymentDue = DateTime.UtcNow.AddDays(-3),
        //            Status = InvoiceStatus.Overdue,
        //            InvoiceType = InvoiceType.Standard,
        //            CustomerId = 10,
        //            CompanyId = 5,
        //            Subtotal = 1600.00m,
        //            Tax = 160.00m,
        //            TotalAmount = 1760.00m,
        //            Notes = "Overdue payment reminder sent.",
        //            PaymentMethod = "Credit Card",
        //            CreatedBy = "system",
        //            CreatedDate = DateTime.UtcNow.AddDays(-18)
        //        },
        //        new Invoice
        //        {
        //            Id = 11,
        //            InvoiceNumber = "INV-011",
        //            PONumber = "PO-011",
        //            IssueDate = DateTime.UtcNow.AddDays(-7),
        //            PaymentDue = DateTime.UtcNow.AddDays(8),
        //            Status = InvoiceStatus.Sent,
        //            InvoiceType = InvoiceType.Standard,
        //            CustomerId = 11,
        //            CompanyId = 5,
        //            Subtotal = 2200.00m,
        //            Tax = 220.00m,
        //            TotalAmount = 2420.00m,
        //            Notes = "Invoice for consulting services.",
        //            PaymentMethod = "PayPal",
        //            CreatedBy = "system",
        //            CreatedDate = DateTime.UtcNow.AddDays(-7)
        //        },
        //        new Invoice
        //        {
        //            Id = 12,
        //            InvoiceNumber = "INV-012",
        //            PONumber = "PO-012",
        //            IssueDate = DateTime.UtcNow.AddDays(-2),
        //            PaymentDue = DateTime.UtcNow.AddDays(13),
        //            Status = InvoiceStatus.Draft,
        //            InvoiceType = InvoiceType.Proforma,
        //            CustomerId = 12,
        //            CompanyId = 6,
        //            Subtotal = 1900.00m,
        //            Tax = 190.00m,
        //            TotalAmount = 2090.00m,
        //            Notes = "Draft for client review.",
        //            PaymentMethod = null,
        //            CreatedBy = "system",
        //            CreatedDate = DateTime.UtcNow.AddDays(-2)
        //        }
        //    );

        //    // Seed Invoice Items
        //    modelBuilder.Entity<InvoiceItem>().HasData(
        //        new InvoiceItem { Id = 1, InvoiceId = 1, Description = "Web Development Services", Quantity = 10, UnitPrice = 100.00m, Amount = 1000.00m, TaxType = "GST", TaxAmount = 100.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-30) },
        //        new InvoiceItem { Id = 2, InvoiceId = 2, Description = "Software License", Quantity = 5, UnitPrice = 300.00m, Amount = 1500.00m, TaxType = "GST", TaxAmount = 150.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-20) },
        //        new InvoiceItem { Id = 3, InvoiceId = 3, Description = "Consulting Services", Quantity = 8, UnitPrice = 100.00m, Amount = 800.00m, TaxType = "VAT", TaxAmount = 80.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-10) },
        //        new InvoiceItem { Id = 4, InvoiceId = 4, Description = "Hardware Installation", Quantity = 20, UnitPrice = 100.00m, Amount = 2000.00m, TaxType = "GST", TaxAmount = 200.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-5) },
        //        new InvoiceItem { Id = 5, InvoiceId = 5, Description = "Annual Maintenance", Quantity = 1, UnitPrice = 3000.00m, Amount = 3000.00m, TaxType = "GST", TaxAmount = 300.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-25) },
        //        new InvoiceItem { Id = 6, InvoiceId = 6, Description = "Subscription Fee", Quantity = 12, UnitPrice = 100.00m, Amount = 1200.00m, TaxType = "GST", TaxAmount = 120.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-15) },
        //        new InvoiceItem { Id = 7, InvoiceId = 7, Description = "Marketing Campaign", Quantity = 18, UnitPrice = 100.00m, Amount = 1800.00m, TaxType = "VAT", TaxAmount = 180.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-8) },
        //        new InvoiceItem { Id = 8, InvoiceId = 8, Description = "Credit for Returned Items", Quantity = 5, UnitPrice = -100.00m, Amount = -500.00m, TaxType = "", TaxAmount = 0.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-3) },
        //        new InvoiceItem { Id = 9, InvoiceId = 9, Description = "Graphic Design Services", Quantity = 25, UnitPrice = 100.00m, Amount = 2500.00m, TaxType = "GST", TaxAmount = 250.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-12) },
        //        new InvoiceItem { Id = 10, InvoiceId = 10, Description = "Cloud Hosting", Quantity = 16, UnitPrice = 100.00m, Amount = 1600.00m, TaxType = "GST", TaxAmount = 160.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-18) },
        //        new InvoiceItem { Id = 11, InvoiceId = 11, Description = "Consulting Services", Quantity = 22, UnitPrice = 100.00m, Amount = 2200.00m, TaxType = "VAT", TaxAmount = 220.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-7) },
        //        new InvoiceItem { Id = 12, InvoiceId = 12, Description = "Software Development", Quantity = 19, UnitPrice = 100.00m, Amount = 1900.00m, TaxType = "GST", TaxAmount = 190.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-2) }
        //    );

        //    // Seed Tax Details
        //    modelBuilder.Entity<TaxDetail>().HasData(
        //        new TaxDetail { Id = 1, InvoiceId = 1, TaxType = "GST", Rate = 10.00m, Amount = 100.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-30) },
        //        new TaxDetail { Id = 2, InvoiceId = 2, TaxType = "GST", Rate = 10.00m, Amount = 150.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-20) },
        //        new TaxDetail { Id = 3, InvoiceId = 3, TaxType = "VAT", Rate = 10.00m, Amount = 80.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-10) },
        //        new TaxDetail { Id = 4, InvoiceId = 4, TaxType = "GST", Rate = 10.00m, Amount = 200.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-5) },
        //        new TaxDetail { Id = 5, InvoiceId = 5, TaxType = "GST", Rate = 10.00m, Amount = 300.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-25) },
        //        new TaxDetail { Id = 6, InvoiceId = 6, TaxType = "GST", Rate = 10.00m, Amount = 120.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-15) },
        //        new TaxDetail { Id = 7, InvoiceId = 7, TaxType = "VAT", Rate = 10.00m, Amount = 180.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-8) },
        //        new TaxDetail { Id = 8, InvoiceId = 9, TaxType = "GST", Rate = 10.00m, Amount = 250.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-12) },
        //        new TaxDetail { Id = 9, InvoiceId = 10, TaxType = "GST", Rate = 10.00m, Amount = 160.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-18) },
        //        new TaxDetail { Id = 10, InvoiceId = 11, TaxType = "VAT", Rate = 10.00m, Amount = 220.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-7) },
        //        new TaxDetail { Id = 11, InvoiceId = 12, TaxType = "GST", Rate = 10.00m, Amount = 190.00m, CreatedBy = "system", CreatedDate = DateTime.UtcNow.AddDays(-2) }
        //    );

        //    // Seed Tax Types
        //    modelBuilder.Entity<TaxType>().HasData(
        //        new TaxType { Id = 1, Name = "Sales 5%", Rate = 5.00m, CompanyId = 1, CreatedBy = "system", CreatedDate = DateTime.UtcNow },
        //        new TaxType { Id = 2, Name = "GST 10%", Rate = 10.00m, CompanyId = 1, CreatedBy = "system", CreatedDate = DateTime.UtcNow },
        //        new TaxType { Id = 3, Name = "VAT 20%", Rate = 20.00m, CompanyId = 2, CreatedBy = "system", CreatedDate = DateTime.UtcNow }
        //    );
        //}
    }
}