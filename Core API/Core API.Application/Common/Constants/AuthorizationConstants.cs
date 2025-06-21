namespace Core_API.Application.Common.Constants
{
    public static class AuthorizationConstants
    {
        // Entity names
        public static class Entities
        {
            public const string Product = "Product";
            public const string Category = "Category";
            public const string Order = "Order";
            public const string Invoice = "Invoice";
            public const string User = "User";
            public const string Customer = "Customer";
            public const string Company = "Company";
            public const string Brand = "Brand";
        }

        // Action types
        public static class Actions
        {
            public const string View = "View";
            public const string Create = "Create";
            public const string Edit = "Edit";
            public const string Delete = "Delete";
            public const string Manage = "Manage";    // Implies all permissions for the entity
            public const string Export = "Export";
            public const string Import = "Import";
            public const string Approve = "Approve";
            public const string Reject = "Reject";
            public const string Process = "Process";
        }
        public static class Permissions
        {
            // Product permissions
            public const string Product_View = "Product.View";
            public const string Product_Create = "Product.Create";
            public const string Product_Edit = "Product.Edit";
            public const string Product_Delete = "Product.Delete";
            public const string Product_Manage = "Product.Manage";
            public const string Product_Export = "Product.Export";
            public const string Product_Import = "Product.Import";

            // Category permissions
            public const string Category_View = "Category.View";
            public const string Category_Create = "Category.Create";
            public const string Category_Edit = "Category.Edit";
            public const string Category_Delete = "Category.Delete";
            public const string Category_Manage = "Category.Manage";

            // Order permissions
            public const string Order_View = "Order.View";
            public const string Order_Create = "Order.Create";
            public const string Order_Edit = "Order.Edit";
            public const string Order_Delete = "Order.Delete";
            public const string Order_Process = "Order.Process";
            public const string Order_Manage = "Order.Manage";
            public const string Order_Export = "Order.Export";

            // Invoice permissions
            public const string Invoice_View = "Invoice.View";
            public const string Invoice_Create = "Invoice.Create";
            public const string Invoice_Edit = "Invoice.Edit";
            public const string Invoice_Delete = "Invoice.Delete";
            public const string Invoice_Approve = "Invoice.Approve";
            public const string Invoice_Reject = "Invoice.Reject";
            public const string Invoice_Manage = "Invoice.Manage";

            // User permissions
            public const string User_View = "User.View";
            public const string User_Create = "User.Create";
            public const string User_Edit = "User.Edit";
            public const string User_Delete = "User.Delete";
            public const string User_Manage = "User.Manage";

            // Customer permissions
            public const string Customer_View = "Customer.View";
            public const string Customer_Create = "Customer.Create";
            public const string Customer_Edit = "Customer.Edit";
            public const string Customer_Delete = "Customer.Delete";
            public const string Customer_Manage = "Customer.Manage";

            // Company permissions
            public const string Company_View = "Company.View";
            public const string Company_Create = "Company.Create";
            public const string Company_Edit = "Company.Edit";
            public const string Company_Delete = "Company.Delete";
            public const string Company_Manage = "Company.Manage";

            // Brand permissions
            public const string Brand_View = "Brand.View";
            public const string Brand_Create = "Brand.Create";
            public const string Brand_Edit = "Brand.Edit";
            public const string Brand_Delete = "Brand.Delete";
            public const string Brand_Manage = "Brand.Manage";
        }

        // Default permission assignments by role
        public static readonly Dictionary<string, List<string>> DefaultRolePermissions = new()
        {
            // Super Admin has all permissions
            {
                AppConstants.Role_Admin_Super, new List<string>
                {
                    Permissions.Product_Manage,
                    Permissions.Category_Manage,
                    Permissions.Order_Manage,
                    Permissions.Invoice_Manage,
                    Permissions.User_Manage,
                    Permissions.Customer_Manage,
                    Permissions.Company_Manage,
                    Permissions.Brand_Manage
                }
            },

            // Admin has all permissions except User_Manage
            {
                AppConstants.Role_Admin, new List<string>
                {
                    Permissions.Product_Manage,
                    Permissions.Category_Manage,
                    Permissions.Order_Manage,
                    Permissions.Invoice_Manage,
                    Permissions.Customer_Manage,
                    Permissions.Company_Manage,
                    Permissions.Brand_Manage
                }
            },

            // Managers can view everything and manage most things except users
            {
                AppConstants.Role_Manager, new List<string>
                {
                    Permissions.Product_Manage,
                    Permissions.Category_Manage,
                    Permissions.Order_Manage,
                    Permissions.Invoice_Manage,
                    Permissions.User_View,
                    Permissions.Customer_Manage,
                    Permissions.Company_View,
                    Permissions.Brand_Manage
                }
            },

            // Employees have view and limited edit permissions
            {
                AppConstants.Role_Employee, new List<string>
                {
                    Permissions.Product_View,
                    Permissions.Category_View,
                    Permissions.Order_View,
                    Permissions.Order_Process,
                    Permissions.Invoice_View,
                    Permissions.Customer_View,
                    Permissions.Company_View,
                    Permissions.Brand_View
                }
            },
            
            // Customer Support can view and edit orders and customers
            {
                AppConstants.Role_CustomerSupport, new List<string>
                {
                    Permissions.Order_View,
                    Permissions.Order_Edit,
                    Permissions.Order_Process,
                    Permissions.Customer_View,
                    Permissions.Customer_Edit,
                    Permissions.Product_View,
                    Permissions.Invoice_View
                }
            },
            
            // Vendors can manage their own products
            {
                AppConstants.Role_Vendor, new List<string>
                {
                    Permissions.Product_View,
                    Permissions.Product_Create,
                    Permissions.Product_Edit,
                    Permissions.Order_View,
                    Permissions.Category_View,
                    Permissions.Brand_View
                }
            },
            
            // Companies can view relevant entities
            {
                AppConstants.Role_Company, new List<string>
                {
                    Permissions.Product_View,
                    Permissions.Order_View,
                    Permissions.Invoice_View,
                    Permissions.Category_View,
                    Permissions.Brand_View
                }
            },
            
            // Delivery Agents can view and process orders
            {
                AppConstants.Role_DeliveryAgent, new List<string>
                {
                    Permissions.Order_View,
                    Permissions.Order_Process,
                    Permissions.Customer_View
                }
            },
            
            // Suppliers can manage products
            {
                AppConstants.Role_Supplier, new List<string>
                {
                    Permissions.Product_View,
                    Permissions.Product_Create,
                    Permissions.Product_Edit,
                    Permissions.Category_View,
                    Permissions.Brand_View
                }
            },
            
            // Customers can view products and their own orders
            {
                AppConstants.Role_Customer, new List<string>
                {
                    Permissions.Product_View,
                    Permissions.Order_View,
                    Permissions.Category_View,
                    Permissions.Brand_View
                }
            }
        };
    }
}
