namespace Core_API.Application.Common.Constants
{
    public static class AppConstants
    {
        /// <summary>
        /// Represents the role of a customer in the application.
        ///
        /// Customers are individual users who browse products or services, make purchases,
        /// manage their personal accounts, view order history, and interact with customer support.
        ///
        /// - Browsing products or services.
        /// - Adding items to cart and completing purchases.
        /// - Managing their profile (address, contact details).
        /// - Viewing order history.
        /// - Writing reviews or ratings.
        /// - Contacting support for assistance.
        /// </summary>
        public const string Role_Customer = "Customer";

        /// <summary>
        /// Represents the role of a company user in the application.
        ///
        /// Company users are businesses or organizations interacting on the platform to manage listings,
        /// access business dashboards, and handle business-related activities.
        ///
        /// - Managing product or service listings.
        /// - Viewing sales data and reports.
        /// - Managing inventory and availability.
        /// - Processing and fulfilling orders.
        /// - Communicating with customers.
        /// - Managing company profiles.
        /// - Running promotions or discounts.
        /// </summary>
        public const string Role_Company = "Company";

        /// <summary>
        /// Represents the role of a super administrator in the application.
        ///
        /// Super administrators possess ultimate control over the entire system, including all functionalities
        /// available to regular administrators, as well as advanced and critical system-level operations.
        /// This role typically has unrestricted access and the authority to manage all aspects of the application.
        ///
        /// - Performing all actions available to a regular administrator.
        /// - Managing core system configurations and settings.
        /// - Overseeing and managing all administrators and their permissions.
        /// - Handling critical security configurations and protocols.
        /// - Performing database-level operations and maintenance.
        /// - Accessing and managing all data across the entire application.
        /// - Implementing high-level system updates and maintenance tasks.
        /// - Managing and controlling access to sensitive system features.
        /// </summary>
        public const string Role_Admin_Super = "Super Admin";

        /// <summary>
        /// Represents the role of an administrator in the application.
        ///
        /// Administrators have the highest access level and manage system-wide settings,
        /// users, roles, security, and application configurations.
        ///
        /// - Managing all user accounts and roles.
        /// - Configuring system settings.
        /// - Monitoring system performance.
        /// - Accessing audit logs and reports.
        /// - Implementing and enforcing security.
        /// - Managing overall content or features.
        /// </summary>
        public const string Role_Admin = "Admin";

        /// <summary>
        /// Represents the role of an internal employee in the application.
        ///
        /// Employees perform operational tasks depending on their department, such as order processing,
        /// customer support, inventory management, or data updates.
        ///
        /// - Processing orders and updating status.
        /// - Responding to customer inquiries.
        /// - Updating product or inventory data.
        /// - Generating area-specific reports.
        /// - Using internal tools and dashboards.
        /// </summary>
        public const string Role_Employee = "Employee";

        /// <summary>
        /// Represents the role of a vendor in the application.
        ///
        /// Vendors are third-party sellers who list and manage their own products or services on the platform.
        /// They can handle their own inventory, view their own sales data, and process their own customer orders.
        ///
        /// - Managing own product listings (creation, updates, deletion).
        /// - Viewing and processing their own orders.
        /// - Managing stock levels for their own products.
        /// - Accessing sales reports and analytics for their account.
        /// - Communicating with customers related to their products.
        /// </summary>
        public const string Role_Vendor = "Vendor";

        /// <summary>
        /// Represents the role of a supplier in the application.
        ///
        /// Suppliers provide inventory, raw materials, or goods to the platform, often for internal use or resale.
        /// They typically manage stock levels, supply chain data, and delivery to warehouses or fulfillment centers.
        ///
        /// - Managing supply data and stock levels.
        /// - Updating delivery schedules.
        /// - Coordinating with the internal team on inventory needs.
        /// - Tracking supply orders and fulfillment status.
        /// </summary>
        public const string Role_Supplier = "Supplier";

        /// <summary>
        /// Represents the role of a manager in the application.
        ///
        /// Managers oversee operational teams, such as employees or vendors.
        /// They have elevated access compared to general employees and can supervise, approve, or monitor activities across multiple areas.
        ///
        /// - Overseeing employee or team operations.
        /// - Managing product or service categories.
        /// - Approving or escalating customer or vendor issues.
        /// - Generating and analyzing reports across departments.
        /// - Accessing management dashboards and tools.
        /// </summary>
        public const string Role_Manager = "Manager";

        /// <summary>
        /// Represents the role of a delivery agent in the application.
        ///
        /// Delivery agents handle the physical delivery of products to customers.
        /// They update delivery statuses, manage their assigned orders, and ensure timely and accurate deliveries.
        ///
        /// - Viewing assigned delivery orders.
        /// - Updating delivery status (e.g., picked up, in transit, delivered).
        /// - Reporting delivery issues.
        /// - Accessing delivery schedules and customer addresses.
        /// </summary>
        public const string Role_DeliveryAgent = "Delivery Agent";

        /// <summary>
        /// Represents the role of a customer support agent in the application.
        ///
        /// Customer support agents assist customers with inquiries, complaints, returns, and other post-sale services.
        /// They have access to customer orders, communication tools, and resolution workflows.
        ///
        /// - Responding to customer inquiries and complaints.
        /// - Processing returns, refunds, or exchanges.
        /// - Tracking customer issues and resolutions.
        /// - Accessing customer order details.
        /// - Using support tools and dashboards to manage tickets.
        /// </summary>
        public const string Role_CustomerSupport = "Customer Support";

        /// <summary>
        // Key used to store the shopping cart data in the session.
        /// </summary>
        public const string SessionCart = "SessionShoppingCart";

    }
}
