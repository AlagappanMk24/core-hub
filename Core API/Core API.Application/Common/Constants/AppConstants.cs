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

        public const string Role_User = "User";

        /// <summary>
        // Key used to store the shopping cart data in the session.
        /// </summary>
        public const string SessionCart = "SessionShoppingCart";

    }
}
