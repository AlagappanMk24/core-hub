using Core_API.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Data.Seeders
{
    public static class DatabaseSeeder
    {
        public static void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Brands
            modelBuilder.Entity<Brand>().HasData(
                new Brand { Id = 1, Name = "Tech Solutions", Slug = "tech-solutions", Description = "Innovative technology solutions.", WebsiteUrl = "https://www.techsolutions.com", LogoUrl = "/images/brands/tech-solutions-logo.png", Country = "United States", EstablishedYear = 2005, IsActive = true },
                new Brand { Id = 2, Name = "Fashion Forward", Slug = "fashion-forward", Description = "Trendy clothing.", WebsiteUrl = "https://www.fashionforward.com", LogoUrl = "/images/brands/fashion-forward-logo.png", Country = "France", EstablishedYear = 2010, IsActive = true },
                new Brand { Id = 3, Name = "Green Living", Slug = "green-living", Description = "Eco-friendly products.", WebsiteUrl = "https://www.greenliving.com", LogoUrl = "/images/brands/green-living-logo.png", Country = "Canada", EstablishedYear = 2015, IsActive = true },
                new Brand { Id = 4, Name = "Global Reads", Slug = "global-reads", Description = "Quality books.", WebsiteUrl = "https://www.globalreads.com", LogoUrl = "/images/brands/global-reads-logo.png", Country = "United Kingdom", EstablishedYear = 1995, IsActive = true },
                new Brand { Id = 5, Name = "Adventure Gear", Slug = "adventure-gear", Description = "Outdoor equipment.", WebsiteUrl = "https://www.adventuregear.com", LogoUrl = "/images/brands/adventure-gear-logo.png", Country = "Australia", EstablishedYear = 2008, IsActive = true },
                new Brand { Id = 6, Name = "Glow & Glam", Slug = "glow-and-glam", Description = "Premium beauty products.", WebsiteUrl = "https://www.glowandglam.com", LogoUrl = "/images/brands/glow-and-glam-logo.png", Country = "South Korea", EstablishedYear = 2012, IsActive = true },
                new Brand { Id = 7, Name = "Fun Time Toys", Slug = "fun-time-toys", Description = "Creative toys.", WebsiteUrl = "https://www.funtimetoys.com", LogoUrl = "/images/brands/fun-time-toys-logo.png", Country = "Germany", EstablishedYear = 2000, IsActive = true },
                new Brand { Id = 8, Name = "Health Hub", Slug = "health-hub", Description = "Health and wellness products.", WebsiteUrl = "https://www.healthhub.com", LogoUrl = "/images/brands/health-hub-logo.png", Country = "United States", EstablishedYear = 2018, IsActive = true },
                new Brand { Id = 9, Name = "Pet Paradise", Slug = "pet-paradise", Description = "Pet care essentials.", WebsiteUrl = "https://www.petparadise.com", LogoUrl = "/images/brands/pet-paradise-logo.png", Country = "United States", EstablishedYear = 2016, IsActive = true },
                new Brand { Id = 10, Name = "Gourmet Delights", Slug = "gourmet-delights", Description = "Premium gourmet foods.", WebsiteUrl = "https://www.gourmetdelights.com", LogoUrl = "/images/brands/gourmet-delights-logo.png", Country = "Italy", EstablishedYear = 2007, IsActive = true },
                new Brand { Id = 11, Name = "Green Living Furniture", Slug = "green-living-furniture", Description = "Eco-friendly furniture for modern homes.", WebsiteUrl = "https://www.greenliving.com/furniture", LogoUrl = "/images/brands/green-living-furniture-logo.png", Country = "Canada", EstablishedYear = 2018, IsActive = true }
            );

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics", DisplayOrder = 1, IsActive = true, Description = "Explore the latest gadgets and electronic devices." },
                new Category { Id = 2, Name = "Apparel", DisplayOrder = 2, IsActive = true, Description = "Discover stylish clothing and accessories for all occasions." },
                new Category { Id = 3, Name = "Home & Garden", DisplayOrder = 3, IsActive = true, Description = "Find everything you need for your home and outdoor spaces." },
                new Category { Id = 4, Name = "Books", DisplayOrder = 4, IsActive = true, Description = "Immerse yourself in captivating stories and knowledge." },
                new Category { Id = 5, Name = "Sports & Outdoors", DisplayOrder = 5, IsActive = true, Description = "Gear up for your active lifestyle and outdoor adventures." },
                new Category { Id = 6, Name = "Beauty & Personal Care", DisplayOrder = 6, IsActive = true, Description = "Enhance your natural beauty and well-being." },
                new Category { Id = 7, Name = "Toys & Games", DisplayOrder = 7, IsActive = true, Description = "Unleash fun and creativity for all ages." },
                new Category { Id = 8, Name = "Health & Wellness", DisplayOrder = 8, IsActive = true, Description = "Support your health with quality supplements and devices." },
                new Category { Id = 9, Name = "Pet Supplies", DisplayOrder = 9, IsActive = true, Description = "Everything you need for your furry friends." },
                new Category { Id = 10, Name = "Food & Beverages", DisplayOrder = 10, IsActive = true, Description = "Savor gourmet foods and premium drinks." }
            );

            // Seed SubCategories
            modelBuilder.Entity<SubCategory>().HasData(
               new SubCategory { Id = 1, Name = "Smartphones", CategoryId = 1, Description = "The latest smartphones from top brands." },
               new SubCategory { Id = 2, Name = "Laptops", CategoryId = 1, Description = "Powerful laptops for work and play." },
               new SubCategory { Id = 3, Name = "Televisions", CategoryId = 1, Description = "High-definition televisions for home entertainment." },
               new SubCategory { Id = 4, Name = "Menswear", CategoryId = 2, Description = "Stylish clothing for men." },
               new SubCategory { Id = 5, Name = "Womenswear", CategoryId = 2, Description = "Trendy clothing for women." },
               new SubCategory { Id = 6, Name = "Kitchen Appliances", CategoryId = 3, Description = "Essential appliances for your kitchen." },
               new SubCategory { Id = 7, Name = "Garden Tools", CategoryId = 3, Description = "Tools for maintaining your garden." },
               new SubCategory { Id = 8, Name = "Fiction", CategoryId = 4, Description = "Imaginative and engaging fictional works." },
               new SubCategory { Id = 9, Name = "Non-Fiction", CategoryId = 4, Description = "Informative and factual books on various topics." },
               new SubCategory { Id = 10, Name = "Camping & Hiking", CategoryId = 5, Description = "Gear for your outdoor adventures." },
               new SubCategory { Id = 11, Name = "Fitness", CategoryId = 5, Description = "Equipment and accessories for your fitness journey." },
               new SubCategory { Id = 12, Name = "Audio & Headphones", CategoryId = 1, Description = "High-quality audio devices and accessories." },
               new SubCategory { Id = 13, Name = "Accessories", CategoryId = 2, Description = "Fashionable accessories to complete your look." },
               new SubCategory { Id = 14, Name = "Furniture", CategoryId = 3, Description = "Stylish and functional furniture for your home." },
               new SubCategory { Id = 15, Name = "Children’s Books", CategoryId = 4, Description = "Engaging books for young readers." },
               new SubCategory { Id = 16, Name = "Cycling", CategoryId = 5, Description = "Bikes and accessories for cycling enthusiasts." },
               new SubCategory { Id = 17, Name = "Skincare", CategoryId = 6, Description = "Products for radiant and healthy skin." },
               new SubCategory { Id = 18, Name = "Haircare", CategoryId = 6, Description = "Solutions for strong and shiny hair." },
               new SubCategory { Id = 19, Name = "Pet Food", CategoryId = 9, Description = "Nutritious food for pets." },
               new SubCategory { Id = 20, Name = "Pet Toys", CategoryId = 9, Description = "Fun toys for your pets." },
               new SubCategory { Id = 21, Name = "Gourmet Snacks", CategoryId = 10, Description = "Premium snacks for every occasion." }
            );

            // Seed Companies
            modelBuilder.Entity<Company>().HasData(
                new Company
                {
                    Id = 1,
                    Name = "Tech Solutions Inc.",
                    StreetAddress = "123 Innovation Way",
                    City = "Silicon City",
                    State = "CA",
                    PostalCode = "94016",
                    PhoneNumber = "555-123-4567"
                },
                new Company
                {
                    Id = 2,
                    Name = "Fashion Forward Ltd.",
                    StreetAddress = "456 Style Avenue",
                    City = "Fashionville",
                    State = "NY",
                    PostalCode = "10001",
                    PhoneNumber = "212-987-6543"
                },
                new Company
                {
                    Id = 3,
                    Name = "Green Living Co.",
                    StreetAddress = "789 Earth Street",
                    City = "Eco City",
                    State = "GA",
                    PostalCode = "30303",
                    PhoneNumber = "404-555-7890"
                },
                new Company
                {
                    Id = 4,
                    Name = "Global Reads",
                    StreetAddress = "101 Literary Lane",
                    City = "Booktown",
                    State = "IL",
                    PostalCode = "60602",
                    PhoneNumber = "312-555-1122"
                },
                new Company
                {
                    Id = 5,
                    Name = "Adventure Gear Corp.",
                    StreetAddress = "222 Trail Road",
                    City = "Outdoorsville",
                    State = "CO",
                    PostalCode = "80202",
                    PhoneNumber = "720-555-3344"
                },
                new Company
                {
                    Id = 6,
                    Name = "Glow & Glam",
                    StreetAddress = "333 Radiant Road",
                    City = "Cosmetic City",
                    State = "CA",
                    PostalCode = "90210",
                    PhoneNumber = "310-555-0011"
                },
                new Company
                {
                    Id = 7,
                    Name = "Fun Time Toys",
                    StreetAddress = "444 Playful Place",
                    City = "Toyland",
                    State = "NY",
                    PostalCode = "11201",
                    PhoneNumber = "718-555-9988"
                },
                new Company
                {
                    Id = 8,
                    Name = "Health Hub Inc.",
                    StreetAddress = "555 Wellness Way",
                    City = "Healthville",
                    State = "TX",
                    PostalCode = "77002",
                    PhoneNumber = "713-555-2233"
                },
                new Company
                {
                    Id = 9,
                    Name = "Pet Paradise",
                    StreetAddress = "666 Paw Street",
                    City = "Pettown",
                    State = "FL",
                    PostalCode = "33101",
                    PhoneNumber = "305-555-4455"
                },
                new Company
                {
                    Id = 10,
                    Name = "Gourmet Delights",
                    StreetAddress = "777 Flavor Lane",
                    City = "Foodcity",
                    State = "CA",
                    PostalCode = "94105",
                    PhoneNumber = "415-555-6677"
                }
            );

            // Seed Vendors
            modelBuilder.Entity<Vendor>().HasData(
                new Vendor
                {
                    Id = 1,
                    VendorName = "Tech Solutions Vendor",
                    Email = "vendor@techsolutions.com",
                    PhoneNumber = "555-123-4567",
                    VendorPictureUrl = "/images/vendors/tech-solutions.png",
                    CompanyId = 1 // Tech Solutions Inc.
                },
                new Vendor
                {
                    Id = 2,
                    VendorName = "Fashion Forward Vendor",
                    Email = "vendor@fashionforward.com",
                    PhoneNumber = "212-987-6543",
                    VendorPictureUrl = "/images/vendors/fashion-forward.png",
                    CompanyId = 2 // Fashion Forward Ltd.
                },
                new Vendor
                {
                    Id = 3,
                    VendorName = "Green Living Vendor",
                    Email = "vendor@greenliving.com",
                    PhoneNumber = "404-555-7890",
                    VendorPictureUrl = "/images/vendors/green-living.png",
                    CompanyId = 3 // Green Living Co.
                },
                new Vendor
                {
                    Id = 4,
                    VendorName = "Global Reads Vendor",
                    Email = "vendor@globalreads.com",
                    PhoneNumber = "312-555-1122",
                    VendorPictureUrl = "/images/vendors/global-reads.png",
                    CompanyId = 4 // Global Reads
                },
                new Vendor
                {
                    Id = 5,
                    VendorName = "Adventure Gear Vendor",
                    Email = "vendor@adventuregear.com",
                    PhoneNumber = "720-555-3344",
                    VendorPictureUrl = "/images/vendors/adventure-gear.png",
                    CompanyId = 5 // Adventure Gear Corp.
                },
                new Vendor
                {
                    Id = 6,
                    VendorName = "Glow & Glam Vendor",
                    Email = "vendor@glowandglam.com",
                    PhoneNumber = "310-555-0011",
                    VendorPictureUrl = "/images/vendors/glow-and-glam.png",
                    CompanyId = 6 // Glow & Glam
                },
                new Vendor
                {
                    Id = 7,
                    VendorName = "Fun Time Toys Vendor",
                    Email = "vendor@funtimetoys.com",
                    PhoneNumber = "718-555-9988",
                    VendorPictureUrl = "/images/vendors/fun-time-toys.png",
                    CompanyId = 7 // Fun Time Toys
                },
                new Vendor
                {
                    Id = 8,
                    VendorName = "Health Hub Vendor",
                    Email = "vendor@healthhub.com",
                    PhoneNumber = "713-555-2233",
                    VendorPictureUrl = "/images/vendors/health-hub.png",
                    CompanyId = 8
                },
                new Vendor
                {
                    Id = 9,
                    VendorName = "Pet Paradise Vendor",
                    Email = "vendor@petparadise.com",
                    PhoneNumber = "305-555-4455",
                    VendorPictureUrl = "/images/vendors/pet-paradise.png",
                    CompanyId = 9
                },
                new Vendor
                {
                    Id = 10,
                    VendorName = "Gourmet Delights Vendor",
                    Email = "vendor@gourmetdelights.com",
                    PhoneNumber = "415-555-6677",
                    VendorPictureUrl = "/images/vendors/gourmet-delights.png",
                    CompanyId = 10
                },
                new Vendor
                {
                    Id = 11,
                    VendorName = "TechTrend Innovations",
                    Email = "vendor@techtrend.com",
                    PhoneNumber = "510-555-8899",
                    VendorPictureUrl = "/images/vendors/techtrend.png",
                    CompanyId = 1
                },
                new Vendor
                {
                    Id = 12,
                    VendorName = "EcoStyle Outfitters",
                    Email = "vendor@ecostyle.com",
                    PhoneNumber = "646-555-1122",
                    VendorPictureUrl = "/images/vendors/ecostyle.png",
                    CompanyId = 2
                }
            );

            // Seed Products
            modelBuilder.Entity<Product>().HasData(
               new Product
               {
                   Id = 1,
                   Title = "Smart TV 55 inch 4K UHD with HDR",
                   Description = "Immerse yourself in stunning visuals with this 55-inch 4K Ultra HD Smart TV. Featuring High Dynamic Range (HDR) for vibrant colors and deep contrast, built-in Wi-Fi, and access to all your favorite streaming apps. Enjoy a cinematic experience in the comfort of your living room.",
                   ShortDescription = "55-inch 4K Smart TV with HDR and built-in streaming apps",
                   Price = 699.99,
                   DiscountPrice = 219.99,
                   IsDiscounted = true,
                   DiscountStartDate = new DateTime(2025, 4, 1),
                   DiscountEndDate = new DateTime(2025, 5, 15),
                   StockQuantity = 50,
                   AllowBackorder = false,
                   SKU = "ELC-SMTV-001",
                   Barcode = "789012345678",
                   CategoryId = 1,
                   SubCategoryId = 3, // Televisions
                   BrandId = 1,
                   VendorId = 1,
                   WeightInKg = 15.5,
                   WidthInCm = 123.5,
                   HeightInCm = 71.2,
                   LengthInCm = 8.3,
                   IsActive = true,
                   IsFeatured = true,
                   IsNewArrival = false,
                   IsTrending = true,
                   MetaTitle = "55 inch 4K Smart TV | Tech Solutions",
                   MetaDescription = "Shop our 55-inch 4K UHD Smart TV with HDR technology for the ultimate home entertainment experience.",
                   Views = 2500,
                   SoldCount = 120,
                   AverageRating = 4.7,
                   TotalReviews = 85
               },
               new Product
               {
                   Id = 2,
                   Title = "Premium Cotton T-Shirt - Men's (Navy Blue)",
                   Description = "Experience ultimate comfort with our premium 100% combed cotton men's t-shirt. Designed for a classic fit and exceptional softness, this navy blue tee is a versatile wardrobe staple perfect for everyday wear. Available in multiple sizes.",
                   ShortDescription = "Premium soft cotton t-shirt for men in navy blue",
                   Price = 24.00,
                   DiscountPrice = 16.99,
                   IsDiscounted = true,
                   DiscountStartDate = new DateTime(2025, 4, 10),
                   DiscountEndDate = new DateTime(2025, 4, 30),
                   StockQuantity = 30,
                   AllowBackorder = true,
                   SKU = "APP-MTSRT-002-NVY",
                   Barcode = "456789012345",
                   CategoryId = 2,
                   SubCategoryId = 4, // Menswear
                   BrandId = 2,
                   VendorId = 2,
                   WeightInKg = 0.2,
                   WidthInCm = 50.0,
                   HeightInCm = 70.0,
                   LengthInCm = 0.5,
                   IsActive = true,
                   IsFeatured = false,
                   IsNewArrival = true,
                   IsTrending = false,
                   MetaTitle = "Men's Premium Cotton T-Shirt | Fashion Forward",
                   MetaDescription = "Classic fit men's navy blue t-shirt made from 100% premium combed cotton for all-day comfort.",
                   Views = 1800,
                   SoldCount = 250,
                   AverageRating = 4.5,
                   TotalReviews = 45
               },
               new Product
               {
                   Id = 3,
                   Title = "Essential Garden Tool Set (3-Piece with Wooden Handles)",
                   Description = "Get your gardening tasks done with ease using our durable 3-piece garden tool set. Includes a sturdy trowel, hand fork, and cultivator, all featuring comfortable wooden handles for a secure grip. Perfect for both novice and experienced gardeners.",
                   ShortDescription = "3-piece garden tool set with wooden handles",
                   Price = 40.00,
                   DiscountPrice = 40.00,
                   IsDiscounted = false,
                   StockQuantity = 30,
                   AllowBackorder = false,
                   SKU = "HGN-TLSET-003-WD",
                   Barcode = "123456789012",
                   CategoryId = 3,
                   SubCategoryId = 7, // Garden Tools
                   BrandId = 3,
                   VendorId = 3,
                   WeightInKg = 0.9,
                   WidthInCm = 12.0,
                   HeightInCm = 30.0,
                   LengthInCm = 5.0,
                   IsActive = true,
                   IsFeatured = false,
                   IsNewArrival = true,
                   IsTrending = false,
                   MetaTitle = "Essential Garden Tool Set | Green Living",
                   MetaDescription = "High-quality 3-piece garden tool set with comfortable wooden handles for all your gardening needs.",
                   Views = 950,
                   SoldCount = 45,
                   AverageRating = 4.8,
                   TotalReviews = 28,
               },
               new Product
               {
                   Id = 4,
                   Title = "Ultra-Slim Laptop 15.6\" with SSD",
                   Description = "Experience lightning-fast performance with our ultra-slim 15.6-inch laptop. Featuring a powerful processor, 512GB SSD storage, and 16GB RAM for seamless multitasking. The vibrant Full HD display and long-lasting battery make it perfect for work and or entertainment on the go.",
                   ShortDescription = "15.6\" ultra-slim laptop with SSD and powerful performance",
                   Price = 999.99,
                   DiscountPrice = 899.99,
                   IsDiscounted = true,
                   DiscountStartDate = new DateTime(2025, 4, 5),
                   DiscountEndDate = new DateTime(2025, 5, 5),
                   StockQuantity = 35,
                   AllowBackorder = false,
                   SKU = "ELC-LPTOP-004",
                   Barcode = "567890123456",
                   CategoryId = 1,
                   SubCategoryId = 2, // Laptops,
                   BrandId = 1,
                   VendorId = 1,
                   WeightInKg = 1.8,
                   WidthInCm = 35.6,
                   HeightInCm = 1.8,
                   LengthInCm = 24.2,
                   IsActive = true,
                   IsFeatured = true,
                   IsNewArrival = true,
                   IsTrending = true,
                   MetaTitle = "Ultra-Slim 15.6\" Laptop | Tech Solutions",
                   MetaDescription = "Powerful and portable 15.6-inch laptop with SSD storage for fast performance anywhere you go.",
                   Views = 3200,
                   SoldCount = 85,
                   AverageRating = 4.6,
                   TotalReviews = 62
               },
               new Product
               {
                   Id = 5,
                   Title = "Designer Leather Handbag - Women's (Black)",
                   Description = "Add a touch of elegance to any outfit with our exclusive designer leather handbag. Crafted from premium genuine leather with stylish gold-tone hardware and multiple interior compartments for organization. The adjustable shoulder strap and handle offer versatile carrying options.",
                   ShortDescription = "Premium black leather handbag with gold accents",
                   Price = 149.99,
                   DiscountPrice = 129.99,
                   IsDiscounted = true,
                   DiscountStartDate = new DateTime(2025, 4, 15),
                   DiscountEndDate = new DateTime(2025, 5, 15),
                   StockQuantity = 25,
                   AllowBackorder = false,
                   SKU = "APP-HBAG-005-BLK",
                   Barcode = "345678901234",
                   CategoryId = 2,
                   SubCategoryId = 5, // Womenswear
                   BrandId = 2,
                   VendorId = 2,
                   WeightInKg = 0.8,
                   WidthInCm = 35.0,
                   HeightInCm = 25.0,
                   LengthInCm = 12.0,
                   IsActive = true,
                   IsFeatured = true,
                   IsNewArrival = false,
                   IsTrending = true,
                   MetaTitle = "Designer Black Leather Handbag | Fashion Forward",
                   MetaDescription = "Elegant black leather handbag with multiple compartments and versatile carrying options.",
                   Views = 1950,
                   SoldCount = 68,
                   AverageRating = 4.8,
                   TotalReviews = 45
               },
               new Product
               {
                   Id = 6,
                   Title = "Premium Digital SLR Camera with 18-55mm Lens",
                   Description = "Capture life's special moments with exceptional clarity using our premium DSLR camera. Features a high-quality 24.1 megapixel CMOS sensor, 4K video recording, and includes a versatile 18-55mm lens. Perfect for both amateur and professional photography enthusiasts.",
                   ShortDescription = "High-resolution 24.1MP DSLR camera with 4K video and 18-55mm lens",
                   Price = 899.99,
                   DiscountPrice = 799.99,
                   IsDiscounted = true,
                   DiscountStartDate = new DateTime(2025, 3, 20),
                   DiscountEndDate = new DateTime(2025, 4, 30),
                   StockQuantity = 25,
                   AllowBackorder = false,
                   SKU = "ELC-CAM-006",
                   Barcode = "234567890123",
                   CategoryId = 1,
                   SubCategoryId = 12, // Audio & Cameras (more appropriate subcategory)
                   BrandId = 1,
                   VendorId = 1,
                   WeightInKg = 0.7,
                   WidthInCm = 12.9,
                   HeightInCm = 10.0,
                   LengthInCm = 7.8,
                   IsActive = true,
                   IsFeatured = true,
                   IsNewArrival = false,
                   IsTrending = false,
                   MetaTitle = "Premium DSLR Camera with Lens | Tech Solutions",
                   MetaDescription = "Professional-grade DSLR camera with 24.1MP sensor and 4K video capability for stunning photos and videos.",
                   Views = 1680,
                   SoldCount = 42,
                   AverageRating = 4.9,
                   TotalReviews = 36
               },
               new Product
               {
                   Id = 7,
                   Title = "Organic Green Tea Gift Set (Variety Pack)",
                   Description = "Indulge in the refreshing taste of premium organic green tea with our exclusive gift set. Includes 6 distinct varieties of hand-picked green tea leaves packaged in elegant tins. Perfect for tea enthusiasts or as a thoughtful gift for any special occasion.",
                   ShortDescription = "Organic green tea gift set of 6 premium varieties",
                   Price = 45.00,
                   DiscountPrice = 45.00,
                   IsDiscounted = false,
                   StockQuantity = 40,
                   AllowBackorder = true,
                   SKU = "FBD-TEA-007",
                   Barcode = "890123456789",
                   CategoryId = 10,
                   SubCategoryId = 21, // Gourmet Snacks
                   BrandId = 3,
                   VendorId = 3,
                   WeightInKg = 0.5,
                   WidthInCm = 25.0,
                   HeightInCm = 8.0,
                   LengthInCm = 20.0,
                   IsActive = true,
                   IsFeatured = false,
                   IsNewArrival = true,
                   IsTrending = false,
                   MetaTitle = "Organic Green Tea Gift Set | Green Living",
                   MetaDescription = "Premium selection of 6 organic green tea varieties presented in an elegant gift box.",
                   Views = 890,
                   SoldCount = 38,
                   AverageRating = 4.7,
                   TotalReviews = 22 // Corrected from 226900
               },
               new Product
               {
                   Id = 8,
                   Title = "Mystery Novel Collection",
                   Description = "Dive into suspense with this collection of three gripping mystery novels by bestselling authors. Perfect for fans of thrilling plots and unexpected twists.",
                   ShortDescription = "Set of three mystery novels",
                   Price = 39.99,
                   DiscountPrice = 34.99,
                   IsDiscounted = true,
                   DiscountStartDate = new DateTime(2025, 4, 1),
                   DiscountEndDate = new DateTime(2025, 4, 30),
                   StockQuantity = 50,
                   AllowBackorder = true,
                   SKU = "BOK-MYST-008",
                   Barcode = "978123456789",
                   CategoryId = 4,
                   SubCategoryId = 8, // Fiction
                   BrandId = 4,
                   VendorId = 4,
                   WeightInKg = 1.2,
                   WidthInCm = 15.0,
                   HeightInCm = 22.0,
                   LengthInCm = 6.0,
                   IsActive = true,
                   IsFeatured = false,
                   IsNewArrival = true,
                   IsTrending = false,
                   MetaTitle = "Mystery Novel Collection | Global Reads",
                   MetaDescription = "Three gripping mystery novels for suspenseful reading.",
                   Views = 1200,
                   SoldCount = 60,
                   AverageRating = 4.5,
                   TotalReviews = 35
               },
               new Product
               {
                   Id = 9,
                   Title = "Waterproof Hiking Boots",
                   Description = "Conquer any trail with these durable waterproof hiking boots. Designed for comfort and traction, featuring breathable materials and ankle support.",
                   ShortDescription = "Waterproof hiking boots for all terrains",
                   Price = 129.99,
                   DiscountPrice = 109.99,
                   IsDiscounted = true,
                   DiscountStartDate = new DateTime(2025, 4, 10),
                   DiscountEndDate = new DateTime(2025, 5, 10),
                   StockQuantity = 40,
                   AllowBackorder = false,
                   SKU = "SPO-BOOT-009",
                   Barcode = "123456789014",
                   CategoryId = 5,
                   SubCategoryId = 10, // Camping & Hiking,
                   BrandId = 5,
                   VendorId = 5,
                   WeightInKg = 1.0,
                   WidthInCm = 25.0,
                   HeightInCm = 15.0,
                   LengthInCm = 20.0,
                   IsActive = true,
                   IsFeatured = true,
                   IsNewArrival = true,
                   IsTrending = true,
                   MetaTitle = "Waterproof Hiking Boots | Adventure Gear",
                   MetaDescription = "Durable waterproof hiking boots for outdoor adventures.",
                   Views = 2100,
                   SoldCount = 80,
                   AverageRating = 4.7,
                   TotalReviews = 55
               },
               new Product
               {
                   Id = 10,
                   Title = "Anti-Aging Skincare Collection Set",
                   Description = "Turn back the clock with this comprehensive anti-aging skincare collection. This five-piece set includes cleanser, toner, day cream with SPF 30, night serum, and eye cream, all formulated with powerful ingredients for rejuvenation.",
                   ShortDescription = "Five-piece anti-aging skincare set",
                   Price = 89.99,
                   DiscountPrice = 75.99,
                   IsDiscounted = true,
                   DiscountStartDate = new DateTime(2025, 4, 10),
                   DiscountEndDate = new DateTime(2025, 5, 10),
                   StockQuantity = 30,
                   AllowBackorder = true,
                   SKU = "BPC-AAGE-010",
                   Barcode = "678901234567",
                   CategoryId = 6,
                   SubCategoryId = 17, // Skincare
                   BrandId = 6,
                   VendorId = 6,
                   WeightInKg = 0.6,
                   WidthInCm = 20.0,
                   HeightInCm = 15.0,
                   LengthInCm = 8.0,
                   IsActive = true,
                   IsFeatured = true,
                   IsNewArrival = true,
                   IsTrending = true,
                   MetaTitle = "Anti-Aging Skincare Collection | Glow & Glam",
                   MetaDescription = "Complete 5-piece anti-aging skincare routine for youthful skin.",
                   Views = 2800,
                   SoldCount = 110,
                   AverageRating = 4.7,
                   TotalReviews = 82
               },
               new Product
               {
                   Id = 11,
                   Title = "Interactive Learning Robot for Kids",
                   Description = "Spark your child's interest in STEM with this interactive learning robot. Programmable via an app, it teaches coding and responds to voice commands, growing with your child’s skills.",
                   ShortDescription = "Educational programmable robot for kids Price = 79.99",
                   DiscountPrice = 69.99,
                   IsDiscounted = true,
                   DiscountStartDate = new DateTime(2025, 3, 1),
                   DiscountEndDate = new DateTime(2025, 5, 31),
                   StockQuantity = 25,
                   AllowBackorder = false,
                   SKU = "TOY-ROBOT-011",
                   Barcode = "789012345670",
                   CategoryId = 7,
                   SubCategoryId = null, // No subcategory
                   BrandId = 7,
                   VendorId = 7,
                   WeightInKg = 0.5,
                   WidthInCm = 15.0,
                   HeightInCm = 22.0,
                   LengthInCm = 12.0,
                   IsActive = true,
                   IsFeatured = true,
                   IsNewArrival = true,
                   IsTrending = true,
                   MetaTitle = "Interactive Learning Robot | Fun Time Toys",
                   MetaDescription = "Educational robot for kids aged 6-12 to learn coding and STEM.",
                   Views = 3500,
                   SoldCount = 135,
                   AverageRating = 4.9,
                   TotalReviews = 98
               },
               new Product
               {
                   Id = 12,
                   Title = "Smartphone Stabilizer Gimbal",
                   Description = "Elevate your smartphone mobile videography with this advanced 3-axis smartphone stabilizer gimbal. Features intelligent tracking and a foldable design for portability.",
                   ShortDescription = "Portable 3-axis smartphone gimbal stabilizer",
                   Price = 85.00,
                   DiscountPrice = 69.99,
                   IsDiscounted = true,
                   DiscountStartDate = new DateTime(2025, 4, 15),
                   DiscountEndDate = new DateTime(2025, 5, 15),
                   StockQuantity = 40,
                   AllowBackorder = false,
                   SKU = "ELC-GIMB-012",
                   Barcode = "123456789013",
                   CategoryId = 1,
                   SubCategoryId = 12, // Smartphones
                   BrandId = 1,
                   VendorId = 1,
                   WeightInKg = 0.4,
                   WidthInCm = 12.0,
                   HeightInCm = 19.0,
                   LengthInCm = 5.0,
                   IsActive = true,
                   IsFeatured = false,
                   IsNewArrival = true,
                   IsTrending = false,
                   MetaTitle = "Smartphone Stabilizer Gimbal | Tech Solutions",
                   MetaDescription = "Professional 3-axis gimbal for smooth, professional smartphone videos.",
                   Views = 1680,
                   SoldCount = 58,
                   AverageRating = 4.6,
                   TotalReviews = 42
               },
               new Product
               {
                   Id = 13,
                   Title = "Smart Home Starter Kit",
                   Description = "Transform your home with this smart home starter kit, including a hub, smart plugs, sensors, and smart bulbs, all app-controlled via app.",
                   ShortDescription = "Complete smart home automation kit",
                   Price = 179.99,
                   DiscountPrice = 149.99,
                   IsDiscounted = true,
                   DiscountStartDate = new DateTime(2025, 4, 1),
                   DiscountEndDate = new DateTime(2025, 6, 1),
                   StockQuantity = 30,
                   AllowBackorder = true,
                   SKU = "ELC-SMHM-013",
                   Barcode = "234567890124",
                   CategoryId = 1,
                   SubCategoryId = null, // No subcategory
                   BrandId = 1,
                   VendorId = 1,
                   WeightInKg = 1.2,
                   WidthInCm = 30.0,
                   HeightInCm = 25.0,
                   LengthInCm = 15.0,
                   IsActive = true,
                   IsFeatured = true,
                   IsNewArrival = true,
                   IsTrending = true,
                   MetaTitle = "Smart Home Starter Kit | Tech Solutions",
                   MetaDescription = "Automate your home with this all-in-one smart home starter kit.",
                   Views = 2900,
                   SoldCount = 65,
                   AverageRating = 4.8,
                   TotalReviews = 38
               },
               new Product
               {
                   Id = 14,
                   Title = "Wireless Earbuds with Charging Case",
                   Description = "Enjoy crystal-clear sound and true wireless freedom with these earbuds. Features noise cancellation, up to 24 hours of battery life, and a compact charging case.",
                   ShortDescription = "True wireless earbuds with noise cancellation",
                   Price = 59.99,
                   DiscountPrice = 49.99,
                   IsDiscounted = true,
                   DiscountStartDate = new DateTime(2025, 4, 5),
                   DiscountEndDate = new DateTime(2025, 5, 5),
                   StockQuantity = 60,
                   AllowBackorder = true,
                   SKU = "ELC-EARB-014",
                   Barcode = "345678901235",
                   CategoryId = 1,
                   SubCategoryId = 12, // Audio & Headphones,
                   BrandId = 1,
                   VendorId = 11,
                   WeightInKg = 0.1,
                   WidthInCm = 8.0,
                   HeightInCm = 4.0,
                   LengthInCm = 3.0,
                   IsActive = true,
                   IsFeatured = true,
                   IsNewArrival = true,
                   IsTrending = true,
                   MetaTitle = "Wireless Earbuds | Tech Solutions",
                   MetaDescription = "High-quality wireless earbuds with noise cancellation and long battery life.",
                   Views = 3000,
                   SoldCount = 150,
                   AverageRating = 4.6,
                   TotalReviews = 95
               },
               new Product
               {
                   Id = 15,
                   Title = "Eco-Friendly Yoga Mat",
                   Description = "Practice yoga with peace of mind on this eco-friendly yoga mat made from sustainable materials. Non-slip surface, lightweight, and extra cushioning for comfort.",
                   ShortDescription = "Sustainable non-slip yoga mat",
                   Price = 39.99,
                   DiscountPrice = 34.99,
                   IsDiscounted = true,
                   DiscountStartDate = new DateTime(2025, 4, 10),
                   DiscountEndDate = new DateTime(2025, 5, 10),
                   StockQuantity = 50,
                   AllowBackorder = true,
                   SKU = "SPO-YOGA-015",
                   Barcode = "456789123456",
                   CategoryId = 5,
                   SubCategoryId = 11,
                   // Fitness
                   BrandId = 3,
                   VendorId = 3,
                   WeightInKg = 1.0,
                   WidthInCm = 61.0,
                   HeightInCm = 183.0,
                   LengthInCm = 0.6,
                   IsActive = true,
                   IsFeatured = false,
                   IsNewArrival = true,
                   IsTrending = false,
                   MetaTitle = "Eco-Friendly Yoga Mat | Green Living",
                   MetaDescription = "Sustainable yoga mat with non-slip surface for comfortable practice.",
                   Views = 1400,
                   SoldCount = 70,
                   AverageRating = 4.5,
                   TotalReviews = 40
               },
               new Product
               {
                   Id = 16,
                   Title = "Luxury Silk Tie",
                   Description = "Elevate your formal attire with this luxury silk tie, handcrafted with intricate patterns and a smooth finish.",
                   ShortDescription = "Handcrafted luxury silk tie for men",
                   Price = 45.00,
                   DiscountPrice = 45.00,
                   IsDiscounted = false,
                   StockQuantity = 30,
                   AllowBackorder = true,
                   SKU = "APP-TIE-016",
                   Barcode = "567890123457",
                   CategoryId = 2,
                   SubCategoryId = 13,
                   // Accessories
                   BrandId = 2,
                   VendorId = 12,
                   WeightInKg = 0.1,
                   WidthInCm = 8.0,
                   HeightInCm = 150.0,
                   LengthInCm = 1.0,
                   IsActive = true,
                   IsFeatured = false,
                   IsNewArrival = true,
                   IsTrending = false,
                   MetaTitle = "Luxury Silk Tie | Fashion Forward",
                   MetaDescription = "Premium silk tie for men with elegant design.",
                   Views = 900,
                   SoldCount = 25,
                   AverageRating = 4.7,
                   TotalReviews = 20
               },
               new Product
               {
                   Id = 17,
                   Title = "Pet Bed with Cooling Gel",
                   Description = "Keep your pet comfortable with this orthopedic pet bed, featuring featuring a cooling gel layer for temperature regulation and support.",
                   ShortDescription = "Cooling gel pet bed",
                   Price = 69.99,
                   DiscountPrice = 59.99,
                   IsDiscounted = true,
                   DiscountStartDate = new DateTime(2025, 4, 15),
                   DiscountEndDate = new DateTime(2025, 5, 15),
                   StockQuantity = 20,
                   AllowBackorder = false,
                   SKU = "PET-BED-017",
                   Barcode = "678901234568",
                   CategoryId = 9,
                   SubCategoryId = null, // No subcategory
                   BrandId = 9,
                   VendorId = 9,
                   WeightInKg = 2.0,
                   WidthInCm = 80.0,
                   HeightInCm = 15.0,
                   LengthInCm = 60.0,
                   IsActive = true,
                   IsFeatured = true,
                   IsNewArrival = true,
                   IsTrending = true,
                   MetaTitle = "Cooling Pet Bed | Pet Comfort",
                   MetaDescription = "Orthopedic pet bed for ultimate pet comfort.",
                   Views = 2000,
                   SoldCount = 85,
                   AverageRating = 4.8,
                   TotalReviews = 60
               },
               new Product
               {
                   Id = 18,
                   Title = "Gourmet Chocolate Truffle Box",
                   Description = "Indulge in our handcrafted gourmet chocolate truffle box, featuring featuring 24 assorted premium chocolates made with the finest ingredients.",
                   ShortDescription = "Box of 24 assorted gourmet truffles",
                   Price = 29.99,
                   DiscountPrice = 24.99,
                   IsDiscounted = true,
                   DiscountStartDate = new DateTime(2025, 4, 20),
                   DiscountEndDate = new DateTime(2025, 5, 20),
                   StockQuantity = 50,
                   AllowBackorder = true,
                   SKU = "FOD-CHOC-018",
                   Barcode = "789012345679",
                   CategoryId = 10,
                   SubCategoryId = 21,
                   // Gourmet Snacks
                   BrandId = 10,
                   VendorId = 10,
                   WeightInKg = 0.5,
                   WidthInCm = 20.0,
                   HeightInCm = 5.0,
                   LengthInCm = 20.0,
                   IsActive = true,
                   IsFeatured = true,
                   IsNewArrival = true,
                   IsTrending = true,
                   MetaTitle = "Gourmet Chocolate Truffle Box | Gourmet Delights",
                   MetaDescription = "Handcrafted assorted chocolate truffles for a luxurious treat.",
                   Views = 2500,
                   SoldCount = 90,
                   AverageRating = 4.9,
                   TotalReviews = 70
               },
               new Product
               {
                   Id = 19,
                   Title = "High-Back Office Chair",
                   Description = "Enhance your workspace with this ergonomic high-back office chair, featuring adjustable height, lumbar support, and breathable mesh.",
                   ShortDescription = "Ergonomic high-back office chair",
                   Price = 149.99,
                   DiscountPrice = 129.99,
                   IsDiscounted = true,
                   DiscountStartDate = new DateTime(2025, 4, 5),
                   DiscountEndDate = new DateTime(2025, 5, 5),
                   StockQuantity = 30,
                   AllowBackorder = false,
                   SKU = "HMG-CHAIR-019",
                   Barcode = "890123456780",
                   CategoryId = 3,
                   SubCategoryId = 14,
                   // Furniture
                   BrandId = 11,
                   VendorId = 3,
                   WeightInKg = 15.0,
                   WidthInCm = 65.0,
                   HeightInCm = 50,
                   LengthInCm = 65.0,
                   IsActive = true,
                   IsFeatured = false,
                   IsNewArrival = true,
                   IsTrending = false,
                   MetaTitle = "High-Back Office Chair | Green Living",
                   MetaDescription = "Comfortable ergonomic office chair with lumbar support.",
                   Views = 1100,
                   SoldCount = 45,
                   AverageRating = 4.6,
                   TotalReviews = 30
               },
               new Product
               {
                   Id = 20,
                   Title = "Kids’ Adventure Storybook",
                   Description = "Ignite your child's love for reading with this collection of adventure stories, designed for children ages aged 6-10 with colorful illustrations.",
                   ShortDescription = "Adventure storybook for kids aged 6-10",
                   Price = 19.99,
                   DiscountPrice = 15.99,
                   IsDiscounted = true,
                   DiscountStartDate = new DateTime(2025, 4, 1),
                   DiscountEndDate = new DateTime(2025, 4, 30),
                   StockQuantity = 40,
                   AllowBackorder = true,
                   SKU = "BOK-KIDS-020",
                   Barcode = "901234567891",
                   CategoryId = 4,
                   SubCategoryId = 15, // Children’s Books
                   BrandId = 4,
                   VendorId = 4,
                   WeightInKg = 0.6,
                   WidthInCm = 20.0,
                   HeightInCm = 25.0,
                   LengthInCm = 2.0,
                   IsActive = true,
                   IsFeatured = true,
                   IsNewArrival = true,
                   IsTrending = true,
                   MetaTitle = "Kids’ Adventure Storybook | Global Reads",
                   MetaDescription = "Exciting adventure stories for young readers with vibrant illustrations.",
                   Views = 1800,
                   SoldCount = 65,
                   AverageRating = 4.8,
                   TotalReviews = 45
               },
               new Product
               {
                   Id = 21,
                   Title = "Mountain Bike with Suspension",
                   Description = "Ride with confidence on this high-performance mountain bike, featuring featuring front suspension and durable aluminum frame.",
                   ShortDescription = "High-performance mountain bike",
                   Price = 399.99,
                   DiscountPrice = 349.99,
                   IsDiscounted = true,
                   DiscountStartDate = new DateTime(2025, 4, 15),
                   DiscountEndDate = new DateTime(2025, 5, 15),
                   StockQuantity = 20,
                   AllowBackorder = false,
                   SKU = "SPO-BIKE-021",
                   Barcode = "012345678902",
                   CategoryId = 5,
                   SubCategoryId = 16,
                   // Cycling
                   BrandId = 5,
                   VendorId = 5,
                   WeightInKg = 14.0,
                   WidthInCm = 70.0,
                   HeightInCm = 110.0,
                   LengthInCm = 30,
                   IsActive = true,
                   IsFeatured = true,
                   IsNewArrival = true,
                   IsTrending = true,
                   MetaTitle = "Mountain Bike with Suspension | Adventure Gear",
                   MetaDescription = "High-performance bike with suspension for mountain trails.",
                   Views = 2200,
                   SoldCount = 75,
                   AverageRating = 4.7,
                   TotalReviews = 50
               },
               new Product
               {
                   Id = 22,
                   Title = "Organic Dog Food (5kg)",
                   Description = "Provide your dog with nutritious, organic food made from high-quality ingredients, free from artificial additives.",
                   ShortDescription = "Organic dog food (5kg)",
                   Price = 29.99,
                   DiscountPrice = 24.99,
                   IsDiscounted = true,
                   DiscountStartDate = new DateTime(2025, 4, 20),
                   DiscountEndDate = new DateTime(2025, 5, 20),
                   StockQuantity = 50,
                   AllowBackorder = true,
                   SKU = "PET-FOOD-022",
                   Barcode = "123456789015",
                   CategoryId = 9,
                   SubCategoryId = 19,
                   // Pet Food
                   BrandId = 9,
                   VendorId = 9,
                   WeightInKg = 5.0,
                   WidthInCm = 30.0,
                   HeightInCm = 40.0,
                   LengthInCm = 15.0,
                   IsActive = true,
                   IsFeatured = false,
                   IsNewArrival = true,
                   IsTrending = false,
                   MetaTitle = "Organic Dog Food | Pet Paradise",
                   MetaDescription = "Nutritious organic dog food for your pet’s health.",
                   Views = 1700,
                   SoldCount = 80,
                   AverageRating = 4.6,
                   TotalReviews = 55
               },
               new Product
               {
                   Id = 23,
                   Title = "Natural Shampoo & Conditioner Set",
                   Description = "Nourish your hair with this natural shampoo and conditioner set, formulated with organic ingredients for for healthy scalp and hair.",
                   ShortDescription = "Organic shampoo and conditioner set",
                   Price = 24.99,
                   DiscountPrice = 19.99,
                   IsDiscounted = true,
                   DiscountStartDate = new DateTime(2025, 4, 25),
                   DiscountEndDate = new DateTime(2025, 5, 25),
                   StockQuantity = 40,
                   AllowBackorder = true,
                   SKU = "BPC-SHAMP-023",
                   Barcode = "234567890126",
                   CategoryId = 6,
                   SubCategoryId = 18, // Haircare
                   BrandId = 6,
                   VendorId = 6,
                   WeightInKg = 0.8,
                   WidthInCm = 15.0,
                   HeightInCm = 20.0,
                   LengthInCm = 8.0,
                   IsActive = true,
                   IsFeatured = true,
                   IsNewArrival = true,
                   IsTrending = true,
                   MetaTitle = "Natural Shampoo & Conditioner | Glow & Glam",
                   MetaDescription = "Organic shampoo and conditioner for healthy hair and scalp.",
                   Views = 1900,
                   SoldCount = 100,
                   AverageRating = 4.7,
                   TotalReviews = 70
               }
           );

            // Seed Product Images
            modelBuilder.Entity<ProductImage>().HasData(
                // Images for Smart TV (ProductId = 1)
                new ProductImage { Id = 1, ProductId = 1, ImageUrl = "/images/products/smarttv_main.jpg" },
                new ProductImage { Id = 2, ProductId = 1, ImageUrl = "/images/products/smarttv_side.jpg" },
                new ProductImage { Id = 3, ProductId = 1, ImageUrl = "/images/products/smarttv_ports.jpg" },

                // Images for Cotton T-Shirt (ProductId = 2)
                new ProductImage { Id = 4, ProductId = 2, ImageUrl = "/images/products/tshirt_navy_front.jpg" },
                new ProductImage { Id = 5, ProductId = 2, ImageUrl = "/images/products/tshirt_navy_back.jpg" },
                new ProductImage { Id = 6, ProductId = 2, ImageUrl = "/images/products/tshirt_navy_detail.jpg" },

                // Images for Garden Tool Set (ProductId = 3)
                new ProductImage { Id = 7, ProductId = 3, ImageUrl = "/images/products/gardentools_set.jpg" },
                new ProductImage { Id = 8, ProductId = 3, ImageUrl = "/images/products/gardentools_trowel.jpg" },
                new ProductImage { Id = 9, ProductId = 3, ImageUrl = "/images/products/gardentools_fork.jpg" },

                // Images for Laptop (ProductId = 4)
                new ProductImage { Id = 10, ProductId = 4, ImageUrl = "/images/products/laptop_front.jpg" },
                new ProductImage { Id = 11, ProductId = 4, ImageUrl = "/images/products/laptop_open.jpg" },
                new ProductImage { Id = 12, ProductId = 4, ImageUrl = "/images/products/laptop_side.jpg" },

                // Images for Leather Handbag (ProductId = 5)
                new ProductImage { Id = 13, ProductId = 5, ImageUrl = "/images/products/handbag_black_front.jpg" },
                new ProductImage { Id = 14, ProductId = 5, ImageUrl = "/images/products/handbag_black_side.jpg" },
                new ProductImage { Id = 15, ProductId = 5, ImageUrl = "/images/products/handbag_black_inside.jpg" },

                // Images for Digital SLR Camera (ProductId = 6)
                new ProductImage { Id = 16, ProductId = 6, ImageUrl = "/images/products/camera_front.jpg" },
                new ProductImage { Id = 17, ProductId = 6, ImageUrl = "/images/products/camera_side.jpg" },
                new ProductImage { Id = 18, ProductId = 6, ImageUrl = "/images/products/camera_with_lens.jpg" },

                // Images for Green Tea Set (ProductId = 7)
                new ProductImage { Id = 19, ProductId = 7, ImageUrl = "/images/products/teaset_box.jpg" },
                new ProductImage { Id = 20, ProductId = 7, ImageUrl = "/images/products/teaset_tins.jpg" },
                new ProductImage { Id = 21, ProductId = 7, ImageUrl = "/images/products/teaset_varieties.jpg" },

                // Images for Book (ProductId = 8)
                new ProductImage { Id = 22, ProductId = 8, ImageUrl = "/images/products/mysterybooks_cover.jpg" },
                new ProductImage { Id = 23, ProductId = 8, ImageUrl = "/images/products/mysterybooks_stack.jpg" },
                new ProductImage { Id = 24, ProductId = 8, ImageUrl = "/images/products/mysterybooks_side.jpg" },

                // Images for Hiking Boots (ProductId = 9)
                new ProductImage { Id = 25, ProductId = 9, ImageUrl = "/images/products/hikingboots_pair.jpg" },
                new ProductImage { Id = 26, ProductId = 9, ImageUrl = "/images/products/hikingboots_sole.jpg" },
                new ProductImage { Id = 27, ProductId = 9, ImageUrl = "/images/products/hikingboots_side.jpg" },

                // Images for Skincare Set (ProductId = 10)
                new ProductImage { Id = 28, ProductId = 10, ImageUrl = "/images/products/skincare_set_box.jpg" },
                new ProductImage { Id = 29, ProductId = 10, ImageUrl = "/images/products/skincare_products.jpg" },
                new ProductImage { Id = 30, ProductId = 10, ImageUrl = "/images/products/skincare_ingredients.jpg" },

                // Images for Learning Robot (ProductId = 11)
                new ProductImage { Id = 31, ProductId = 11, ImageUrl = "/images/products/robot_front.jpg" },
                new ProductImage { Id = 32, ProductId = 11, ImageUrl = "/images/products/robot_side.jpg" },
                new ProductImage { Id = 33, ProductId = 11, ImageUrl = "/images/products/robot_app.jpg" },

                // Images for Smartphone Gimbal (ProductId = 12)
                new ProductImage { Id = 34, ProductId = 12, ImageUrl = "/images/products/gimbal_main.jpg" },
                new ProductImage { Id = 35, ProductId = 12, ImageUrl = "/images/products/gimbal_folded.jpg" },
                new ProductImage { Id = 36, ProductId = 12, ImageUrl = "/images/products/gimbal_in_use.jpg" },

                // Images for Smart Home Kit (ProductId = 13)
                new ProductImage { Id = 37, ProductId = 13, ImageUrl = "/images/products/smarthome_kit_box.jpg" },
                new ProductImage { Id = 38, ProductId = 13, ImageUrl = "/images/products/smarthome_components.jpg" },
                new ProductImage { Id = 39, ProductId = 13, ImageUrl = "/images/products/smarthome_app.jpg" },

                // Images for Earbuds (ProductId = 14)
                new ProductImage { Id = 40, ProductId = 14, ImageUrl = "/images/products/earbuds_case.jpg" },
                new ProductImage { Id = 41, ProductId = 14, ImageUrl = "/images/products/earbuds_pair.jpg" },
                new ProductImage { Id = 42, ProductId = 14, ImageUrl = "/images/products/earbuds_charging.jpg" },

                // Images for Yogamat (ProductId = 15)
                new ProductImage { Id = 43, ProductId = 15, ImageUrl = "/images/products/yogamat_top.jpg" },
                new ProductImage { Id = 44, ProductId = 15, ImageUrl = "/images/products/yogamat_rolled.jpg" },
                new ProductImage { Id = 45, ProductId = 15, ImageUrl = "/images/products/yogamat_texture.jpg" },

                // Images for Tie Pattern (ProductId = 16)
                new ProductImage { Id = 46, ProductId = 16, ImageUrl = "/images/products/tie_pattern.jpg" },
                new ProductImage { Id = 47, ProductId = 16, ImageUrl = "/images/products/tie_knot.jpg" },
                new ProductImage { Id = 48, ProductId = 16, ImageUrl = "/images/products/tie_folded.jpg" },

                // Images for Petbed Side (ProductId = 17)
                new ProductImage { Id = 49, ProductId = 17, ImageUrl = "/images/products/petbed_top.jpg" },
                new ProductImage { Id = 50, ProductId = 17, ImageUrl = "/images/products/petbed_side.jpg" },
                new ProductImage { Id = 51, ProductId = 17, ImageUrl = "/images/products/petbed_in_use.jpg" },

                // Images for Chocolates Assorted (ProductId = 18)
                new ProductImage { Id = 52, ProductId = 18, ImageUrl = "/images/products/chocolates_box.jpg" },
                new ProductImage { Id = 53, ProductId = 18, ImageUrl = "/images/products/chocolates_assorted.jpg" },
                new ProductImage { Id = 54, ProductId = 18, ImageUrl = "/images/products/chocolates_closeup.jpg" },

                // Images for Office Chair (ProductId = 19)
                new ProductImage { Id = 55, ProductId = 19, ImageUrl = "/images/products/officechair_front.jpg" },
                new ProductImage { Id = 56, ProductId = 19, ImageUrl = "/images/products/officechair_side.jpg" },
                new ProductImage { Id = 57, ProductId = 19, ImageUrl = "/images/products/officechair_back.jpg" },

                // Images for Storybook (ProductId = 20)
                new ProductImage { Id = 58, ProductId = 20, ImageUrl = "/images/products/storybook_cover.jpg" },
                new ProductImage { Id = 59, ProductId = 20, ImageUrl = "/images/products/storybook_pages.jpg" },
                new ProductImage { Id = 60, ProductId = 20, ImageUrl = "/images/products/storybook_illustrations.jpg" },

                // Images for Mountain Bike (ProductId = 21)
                new ProductImage { Id = 61, ProductId = 21, ImageUrl = "/images/products_mountainbike_frame.jpg" },
                new ProductImage { Id = 62, ProductId = 21, ImageUrl = "/images/products_mountainbike_side.jpg" },
                new ProductImage { Id = 63, ProductId = 21, ImageUrl = "/images/products_mountainbike_tires.jpg" },

                // Images for Dog Food (ProductId = 22)
                new ProductImage { Id = 64, ProductId = 22, ImageUrl = "/images/products/dogfood_bag.jpg" },
                new ProductImage { Id = 65, ProductId = 22, ImageUrl = "/images/products/dogfood_contents.jpg" },
                new ProductImage { Id = 66, ProductId = 22, ImageUrl = "/images/products/dogfood_dog.jpg" },

                // Images for Shampoo (ProductId = 23)
                new ProductImage { Id = 67, ProductId = 23, ImageUrl = "/images/products/shampoo_bottles.jpg", },
                new ProductImage { Id = 68, ProductId = 23, ImageUrl = "/images/products/shampoo_ingredients.jpg" },
                new ProductImage { Id = 69, ProductId = 23, ImageUrl = "/images/products/shampoo_texture.jpg" }
                );
            // Seed Product Specifications
            modelBuilder.Entity<ProductSpecification>().HasData(
                // Specifications for Smart TV (ProductId = 1)
                new ProductSpecification { Id = 1, ProductId = 1, Key = "Screen Size", Value = "55 inches" },
                new ProductSpecification { Id = 2, ProductId = 1, Key = "Resolution", Value = "4K Ultra HD (3840 x 2160)" },
                new ProductSpecification { Id = 3, ProductId = 1, Key = "Display Technology", Value = "LED" },
                new ProductSpecification { Id = 4, ProductId = 1, Key = "HDR", Value = "Yes" },
                new ProductSpecification { Id = 5, ProductId = 1, Key = "Smart TV", Value = "Yes" },
                new ProductSpecification { Id = 6, ProductId = 1, Key = "Connectivity", Value = "Wi-Fi, Bluetooth, HDMI, USB" },

                // Specifications for Cotton T-Shirt (ProductId = 2)
                new ProductSpecification { Id = 7, ProductId = 2, Key = "Material", Value = "100% Combed Cotton" },
                new ProductSpecification { Id = 8, ProductId = 2, Key = "Color", Value = "Navy Blue" },
                new ProductSpecification { Id = 9, ProductId = 2, Key = "Care Instructions", Value = "Machine wash cold, tumble dry low" },

                // Specifications for Garden Tool Set (ProductId = 3)
                new ProductSpecification { Id = 10, ProductId = 3, Key = "Material", Value = "Stainless Steel with Wooden Handles" },
                new ProductSpecification { Id = 11, ProductId = 3, Key = "Pieces", Value = "3" },
                new ProductSpecification { Id = 12, ProductId = 3, Key = "Tool Length", Value = "30 cm" },

                // Specifications for Laptop (ProductId = 4)
                new ProductSpecification { Id = 13, ProductId = 4, Key = "Processor", Value = "Intel Core i7" },
                new ProductSpecification { Id = 14, ProductId = 4, Key = "RAM", Value = "16 GB" },
                new ProductSpecification { Id = 15, ProductId = 4, Key = "Storage", Value = "512 GB SSD" },
                new ProductSpecification { Id = 16, ProductId = 4, Key = "Display", Value = "15.6-inch Full HD (1920 x 1080)" },
                new ProductSpecification { Id = 17, ProductId = 4, Key = "Battery Life", Value = "Up to 10 hours" },
                new ProductSpecification { Id = 18, ProductId = 4, Key = "Operating System", Value = "Windows 11" },

                // Specifications for Leather Handbag (ProductId = 5)
                new ProductSpecification { Id = 19, ProductId = 5, Key = "Material", Value = "Genuine Leather" },
                new ProductSpecification { Id = 20, ProductId = 5, Key = "Color", Value = "Black" },
                new ProductSpecification { Id = 21, ProductId = 5, Key = "Dimensions", Value = "35 x 25 x 12 cm" },
                new ProductSpecification { Id = 22, ProductId = 5, Key = "Hardware", Value = "Gold-tone" },

                // Specifications for Digital SLR Camera (ProductId = 6)
                new ProductSpecification { Id = 23, ProductId = 6, Key = "Megapixels", Value = "24.1 MP" },
                new ProductSpecification { Id = 24, ProductId = 6, Key = "Sensor Type", Value = "CMOS" },
                new ProductSpecification { Id = 25, ProductId = 6, Key = "Video Resolution", Value = "4K" },
                new ProductSpecification { Id = 26, ProductId = 6, Key = "Lens", Value = "18-55mm" },
                new ProductSpecification { Id = 27, ProductId = 6, Key = "ISO Range", Value = "100-25600" },

                // Specifications for Green Tea Set (ProductId = 7)
                new ProductSpecification { Id = 28, ProductId = 7, Key = "Varieties", Value = "6" },
                new ProductSpecification { Id = 29, ProductId = 7, Key = "Organic", Value = "Yes" },
                new ProductSpecification { Id = 30, ProductId = 7, Key = "Weight", Value = "300g total (50g each)" },
                new ProductSpecification { Id = 31, ProductId = 7, Key = "Packaging", Value = "Metal tins in gift box" },

                // Specifications for Book (ProductId = 8)
                new ProductSpecification { Id = 32, ProductId = 8, Key = "Format", Value = "Hardcover" },
                new ProductSpecification { Id = 33, ProductId = 8, Key = "Pages", Value = "384" },
                new ProductSpecification { Id = 34, ProductId = 8, Key = "Genre", Value = "Historical Fiction" },
                new ProductSpecification { Id = 35, ProductId = 8, Key = "Language", Value = "English" },

                // Specifications for Hiking Boots (ProductId = 9)
                new ProductSpecification { Id = 36, ProductId = 9, Key = "Material", Value = "Waterproof Leather and Mesh" },
                new ProductSpecification { Id = 37, ProductId = 9, Key = "Sole", Value = "Rubber with Multi-directional Traction" },
                new ProductSpecification { Id = 38, ProductId = 9, Key = "Closure", Value = "Lace-up" },
                new ProductSpecification { Id = 39, ProductId = 9, Key = "Gender", Value = "Unisex" },

                // Specifications for Skincare Set (ProductId = 10)
                new ProductSpecification { Id = 40, ProductId = 10, Key = "Pieces", Value = "5" },
                new ProductSpecification { Id = 41, ProductId = 10, Key = "Skin Type", Value = "All" },
                new ProductSpecification { Id = 42, ProductId = 10, Key = "Key Ingredients", Value = "Peptides, Hyaluronic Acid, Antioxidants" },
                new ProductSpecification { Id = 43, ProductId = 10, Key = "SPF", Value = "30 (Day Cream)" },

                // Specifications for Learning Robot (ProductId = 11)
                new ProductSpecification { Id = 44, ProductId = 11, Key = "Age Range", Value = "6-12 years" },
                new ProductSpecification { Id = 45, ProductId = 11, Key = "Programmable", Value = "Yes" },
                new ProductSpecification { Id = 46, ProductId = 11, Key = "Battery Life", Value = "4 hours" },
                new ProductSpecification { Id = 47, ProductId = 11, Key = "Connectivity", Value = "Bluetooth" },
                new ProductSpecification { Id = 48, ProductId = 11, Key = "App Compatibility", Value = "iOS and Android" },

                // Specifications for Smartphone Gimbal (ProductId = 12)
                new ProductSpecification { Id = 49, ProductId = 12, Key = "Axes", Value = "3-axis" },
                new ProductSpecification { Id = 50, ProductId = 12, Key = "Battery Life", Value = "12 hours" },
                new ProductSpecification { Id = 51, ProductId = 12, Key = "Compatibility", Value = "Most smartphones up to 6.7 inches" },
                new ProductSpecification { Id = 52, ProductId = 12, Key = "Weight", Value = "400g" },

                // Specifications for Smart Home Kit (ProductId = 13)
                new ProductSpecification { Id = 53, ProductId = 13, Key = "Components", Value = "1 Hub, 2 Plugs, 2 Sensors, 3 Bulbs" },
                new ProductSpecification { Id = 54, ProductId = 13, Key = "Connectivity", Value = "Wi-Fi, Bluetooth" },
                new ProductSpecification { Id = 55, ProductId = 13, Key = "Voice Assistant Compatibility", Value = "Alexa, Google Assistant, Siri" },
                new ProductSpecification { Id = 56, ProductId = 13, Key = "App Control", Value = "Yes" },

                // Specifications for Earbuds (ProductId = 14)
                new ProductSpecification { Id = 57, ProductId = 14, Key = "Battery Life", Value = "24 Hours" },
                new ProductSpecification { Id = 58, ProductId = 14, Key = "Features", Value = "Noise Cancellation" },
                new ProductSpecification { Id = 59, ProductId = 14, Key = "Color", Value = "Black" },

                // Specifications for Yogamat (ProductId = 15)
                new ProductSpecification { Id = 60, ProductId = 15, Key = "Material", Value = "Eco-Friendly TPE" },
                new ProductSpecification { Id = 61, ProductId = 15, Key = "Thickness", Value = "6mm" },
                new ProductSpecification { Id = 62, ProductId = 15, Key = "Dimensions", Value = "61 x 183 cm" },

                // Specifications for Tie Pattern (ProductId = 16)
                new ProductSpecification { Id = 63, ProductId = 16, Key = "Material", Value = "100% Silk" },
                new ProductSpecification { Id = 64, ProductId = 16, Key = "Width", Value = "6 cm" },
                new ProductSpecification { Id = 65, ProductId = 16, Key = "Pattern", Value = "Striped" },

                // Specifications for Pet Bed (ProductId = 17)
                new ProductSpecification { Id = 66, ProductId = 17, Key = "Size", Value = "Medium" },
                new ProductSpecification { Id = 67, ProductId = 17, Key = "Material", Value = "Polyurethane Foam, Gel" },
                new ProductSpecification { Id = 68, ProductId = 17, Key = "Dimensions", Value = "80 x 60 x 15 cm" },

                // Specifications for Chocolates Assorted (ProductId = 18)
                new ProductSpecification { Id = 69, ProductId = 18, Key = "Count", Value = "24 Pieces" },
                new ProductSpecification { Id = 70, ProductId = 18, Key = "Flavors", Value = "Assorted" },
                new ProductSpecification { Id = 71, ProductId = 18, Key = "Packaging", Value = "Gift Box" },

                // Specifications for Office Chair (ProductId = 19)
                new ProductSpecification { Id = 72, ProductId = 19, Key = "Max Weight Capacity", Value = "120 kg" },
                new ProductSpecification { Id = 73, ProductId = 19, Key = "Material", Value = "Mesh, Metal" },
                new ProductSpecification { Id = 74, ProductId = 19, Key = "Adjustments", Value = "Height, Lumbar" },
                new ProductSpecification { Id = 75, ProductId = 19, Key = "Color", Value = "Blue" },

                // Specifications for Storybook (ProductId = 20)
                new ProductSpecification { Id = 76, ProductId = 20, Key = "Age Range", Value = "6-10 Years" },
                new ProductSpecification { Id = 77, ProductId = 20, Key = "Pages", Value = "150" },
                new ProductSpecification { Id = 78, ProductId = 20, Key = "Format", Value = "Hardcover" },

                // Specifications for Mountain Bike (ProductId = 21)
                new ProductSpecification { Id = 79, ProductId = 21, Key = "Frame Material", Value = "Aluminum" },
                new ProductSpecification { Id = 80, ProductId = 21, Key = "Suspension", Value = "Front" },
                new ProductSpecification { Id = 81, ProductId = 21, Key = "Wheel Size", Value = "26 inches" },
                new ProductSpecification { Id = 82, ProductId = 21, Key = "Gears", Value = "21-Speed" },

                // Specifications for Dog Food (ProductId = 22)
                new ProductSpecification { Id = 83, ProductId = 22, Key = "Weight", Value = "5 kg" },
                new ProductSpecification { Id = 84, ProductId = 22, Key = "Ingredients", Value = "Organic Chicken, Vegetables" },
                new ProductSpecification { Id = 85, ProductId = 22, Key = "Type", Value = "Dry Food" },

                // Specifications for Shampoo (ProductId = 23)
                new ProductSpecification { Id = 86, ProductId = 23, Key = "Volume", Value = "500 ml each" },
                new ProductSpecification { Id = 87, ProductId = 23, Key = "Ingredients", Value = "Aloe Vera, Coconut Oil" },
                new ProductSpecification { Id = 88, ProductId = 23, Key = "Type", Value = "Shampoo & Conditioner" }
            );

            // Seed Product Tags
            modelBuilder.Entity<ProductTag>().HasData(
                // Tags for Smart TV (ProductId = 1)
                new ProductTag { Id = 1, ProductId = 1, TagName = "4K" },
                new ProductTag { Id = 2, ProductId = 1, TagName = "Smart TV" },
                new ProductTag { Id = 3, ProductId = 1, TagName = "HDR" },
                new ProductTag { Id = 4, ProductId = 1, TagName = "Home Entertainment" },

                // Tags for Cotton T-Shirt (ProductId = 2)
                new ProductTag { Id = 5, ProductId = 2, TagName = "Men's Fashion" },
                new ProductTag { Id = 6, ProductId = 2, TagName = "Casual Wear" },
                new ProductTag { Id = 7, ProductId = 2, TagName = "Cotton" },

                // Tags for Garden Tool Set (ProductId = 3)
                new ProductTag { Id = 8, ProductId = 3, TagName = "Gardening" },
                new ProductTag { Id = 9, ProductId = 3, TagName = "Tools" },
                new ProductTag { Id = 10, ProductId = 3, TagName = "New Arrival" },

                // Tags for Laptop (ProductId = 4)
                new ProductTag { Id = 11, ProductId = 4, TagName = "Computing" },
                new ProductTag { Id = 12, ProductId = 4, TagName = "SSD" },
                new ProductTag { Id = 13, ProductId = 4, TagName = "Lightweight" },
                new ProductTag { Id = 14, ProductId = 4, TagName = "New Arrival" },

                // Tags for Leather Handbag (ProductId = 5)
                new ProductTag { Id = 15, ProductId = 5, TagName = "Women's Fashion" },
                new ProductTag { Id = 16, ProductId = 5, TagName = "Leather" },
                new ProductTag { Id = 17, ProductId = 5, TagName = "Designer" },
                new ProductTag { Id = 18, ProductId = 5, TagName = "Trending" },

                // Tags for Digital SLR Camera (ProductId = 6)
                new ProductTag { Id = 19, ProductId = 6, TagName = "Photography" },
                new ProductTag { Id = 20, ProductId = 6, TagName = "4K Video" },
                new ProductTag { Id = 21, ProductId = 6, TagName = "Featured" },

                // Tags for Green Tea Set (ProductId = 7)
                new ProductTag { Id = 22, ProductId = 7, TagName = "Organic" },
                new ProductTag { Id = 23, ProductId = 7, TagName = "Gift Set" },
                new ProductTag { Id = 24, ProductId = 7, TagName = "New Arrival" },

                // Tags for Book (ProductId = 8)
                new ProductTag { Id = 25, ProductId = 8, TagName = "Historical Fiction" },
                new ProductTag { Id = 26, ProductId = 8, TagName = "Bestseller" },
                new ProductTag { Id = 27, ProductId = 8, TagName = "New Arrival" },

                // Tags for Hiking Boots (ProductId = 9)
                new ProductTag { Id = 28, ProductId = 9, TagName = "Outdoor" },
                new ProductTag { Id = 29, ProductId = 9, TagName = "Waterproof" },
                new ProductTag { Id = 30, ProductId = 9, TagName = "Unisex" },
                new ProductTag { Id = 31, ProductId = 9, TagName = "Trending" },

                // Tags for Skincare Set (ProductId = 10)
                new ProductTag { Id = 32, ProductId = 10, TagName = "Anti-Aging" },
                new ProductTag { Id = 33, ProductId = 10, TagName = "Beauty" },
                new ProductTag { Id = 34, ProductId = 10, TagName = "New Arrival" },
                new ProductTag { Id = 35, ProductId = 10, TagName = "Trending" },

                // Tags for Learning Robot (ProductId = 11)
                new ProductTag { Id = 36, ProductId = 11, TagName = "Educational" },
                new ProductTag { Id = 37, ProductId = 11, TagName = "STEM" },
                new ProductTag { Id = 38, ProductId = 11, TagName = "Kids" },
                new ProductTag { Id = 39, ProductId = 11, TagName = "New Arrival" },

                // Tags for Smartphone Gimbal (ProductId = 12)
                new ProductTag { Id = 40, ProductId = 12, TagName = "Photography" },
                new ProductTag { Id = 41, ProductId = 12, TagName = "Accessories" },
                new ProductTag { Id = 42, ProductId = 12, TagName = "New Arrival" },

                // Tags for Smart Home Kit (ProductId = 13)
                new ProductTag { Id = 43, ProductId = 13, TagName = "Smart Home" },
                new ProductTag { Id = 44, ProductId = 13, TagName = "IoT" },
                new ProductTag { Id = 45, ProductId = 13, TagName = "New Arrival" },
                new ProductTag { Id = 46, ProductId = 13, TagName = "Trending" },

                // Tags for Earbuds (ProductId = 14)
                new ProductTag { Id = 47, ProductId = 14, TagName = "Audio" },
                new ProductTag { Id = 48, ProductId = 14, TagName = "Wireless" },
                new ProductTag { Id = 49, ProductId = 14, TagName = "Noise Cancelling" },

                // Tags for Yogamat (ProductId = 15)
                new ProductTag { Id = 50, ProductId = 15, TagName = "Fitness" },
                new ProductTag { Id = 51, ProductId = 15, TagName = "Yoga" },
                new ProductTag { Id = 52, ProductId = 15, TagName = "Eco-Friendly" },

                // Tags for Tie Pattern (ProductId = 16)
                new ProductTag { Id = 53, ProductId = 16, TagName = "Men's Accessories" },
                new ProductTag { Id = 54, ProductId = 16, TagName = "Silk" },
                new ProductTag { Id = 55, ProductId = 16, TagName = "Formal Wear" },

                // Tags for Petbed Side (ProductId = 17)
                new ProductTag { Id = 56, ProductId = 17, TagName = "Pet Supplies" },
                new ProductTag { Id = 57, ProductId = 17, TagName = "Comfort" },
                new ProductTag { Id = 58, ProductId = 17, TagName = "Durable" },

                // Tags for Chocolates Assorted (ProductId = 18)
                new ProductTag { Id = 59, ProductId = 18, TagName = "Gourmet" },
                new ProductTag { Id = 60, ProductId = 18, TagName = "Sweets" },
                new ProductTag { Id = 61, ProductId = 18, TagName = "Gift" },

                // Tags for Office Chair (ProductId = 19)
                new ProductTag { Id = 62, ProductId = 19, TagName = "Office Furniture" },
                new ProductTag { Id = 63, ProductId = 19, TagName = "Ergonomic" },
                new ProductTag { Id = 64, ProductId = 19, TagName = "Home Office" },

                // Tags for Storybook (ProductId = 20)
                new ProductTag { Id = 65, ProductId = 20, TagName = "Children's Books" },
                new ProductTag { Id = 66, ProductId = 20, TagName = "Fiction" },
                new ProductTag { Id = 67, ProductId = 20, TagName = "Educational" },

                // Tags for Mountain Bike (ProductId = 21)
                new ProductTag { Id = 68, ProductId = 21, TagName = "Cycling" },
                new ProductTag { Id = 69, ProductId = 21, TagName = "Outdoor" },
                new ProductTag { Id = 70, ProductId = 21, TagName = "Sports" },

                // Tags for Dog Food (ProductId = 22)
                new ProductTag { Id = 71, ProductId = 22, TagName = "Pet Food" },
                new ProductTag { Id = 72, ProductId = 22, TagName = "Organic" },
                new ProductTag { Id = 73, ProductId = 22, TagName = "Dog Supplies" },

                // Tags for Shampoo (ProductId = 23)
                new ProductTag { Id = 74, ProductId = 23, TagName = "Hair Care" },
                new ProductTag { Id = 75, ProductId = 23, TagName = "Beauty" },
                new ProductTag { Id = 76, ProductId = 23, TagName = "Natural Ingredients" },
                new ProductTag { Id = 77, ProductId = 23, TagName = "Shampoo" }
            );

            // Seed Product Variants
            modelBuilder.Entity<ProductVariant>().HasData(
                // Product 1: Smart TV (no variants, single model)
                // Variants for Cotton T-Shirt (ProductId = 2)
                new ProductVariant { Id = 1, ProductId = 2, VariantName = "Size - S", SKU = "APP-MTSRT-002-NVY-S", Price = 20.00, DiscountPrice = 16.99, StockQuantity = 25 },
                new ProductVariant { Id = 2, ProductId = 2, VariantName = "Size - M", SKU = "APP-MTSRT-002-NVY-M", Price = 20.00, DiscountPrice = 16.99, StockQuantity = 30 },
                new ProductVariant { Id = 3, ProductId = 2, VariantName = "Size - L", SKU = "APP-MTSRT-002-NVY-L", Price = 20.00, DiscountPrice = 16.99, StockQuantity = 25 },
                new ProductVariant { Id = 4, ProductId = 2, VariantName = "Size - XL", SKU = "APP-MTSRT-002-NVY-XL", Price = 22.00, DiscountPrice = 18.99, StockQuantity = 20 },

                // Variants for Leather Handbag (ProductId = 5)
                new ProductVariant { Id = 5, ProductId = 5, VariantName = "Color - Black", SKU = "APP-HBAG-005-BLK", Price = 149.99, DiscountPrice = 129.99, StockQuantity = 15 },
                new ProductVariant { Id = 6, ProductId = 5, VariantName = "Color - Brown", SKU = "APP-HBAG-005-BRN", Price = 149.99, DiscountPrice = 129.99, StockQuantity = 10 },

                // Variants for Hiking Boots (ProductId = 9)
                new ProductVariant { Id = 7, ProductId = 9, VariantName = "Size - US 7 / EU 38", SKU = "SPT-HBOOT-009-07", Price = 129.95, DiscountPrice = 109.95, StockQuantity = 8 },
                new ProductVariant { Id = 8, ProductId = 9, VariantName = "Size - US 8 / EU 39", SKU = "SPT-HBOOT-009-08", Price = 129.95, DiscountPrice = 109.95, StockQuantity = 10 },
                new ProductVariant { Id = 9, ProductId = 9, VariantName = "Size - US 9 / EU 40", SKU = "SPT-HBOOT-009-09", Price = 129.95, DiscountPrice = 109.95, StockQuantity = 12 },
                new ProductVariant { Id = 10, ProductId = 9, VariantName = "Size - US 10 / EU 41", SKU = "SPT-HBOOT-009-10", Price = 129.95, DiscountPrice = 109.95, StockQuantity = 10 },
                new ProductVariant { Id = 11, ProductId = 9, VariantName = "Size - US 11 / EU 42", SKU = "SPT-HBOOT-009-11", Price = 129.95, DiscountPrice = 109.95, StockQuantity = 5 },

                // Variants for Smart TV (ProductId = 1)
                new ProductVariant { Id = 12, ProductId = 1, VariantName = "Size - 55 inch", SKU = "ELC-SMTV-001-55", Price = 699.99, DiscountPrice = 649.99, StockQuantity = 30 },
                new ProductVariant { Id = 13, ProductId = 1, VariantName = "Size - 65 inch", SKU = "ELC-SMTV-001-65", Price = 899.99, DiscountPrice = 849.99, StockQuantity = 20 },

                // Variants for Laptop (ProductId = 4)
                new ProductVariant { Id = 14, ProductId = 4, VariantName = "RAM - 16GB / Storage - 512GB", SKU = "ELC-LPTOP-004-16-512", Price = 999.99, DiscountPrice = 899.99, StockQuantity = 20 },
                new ProductVariant { Id = 15, ProductId = 4, VariantName = "RAM - 32GB / Storage - 1TB", SKU = "ELC-LPTOP-004-32-1TB", Price = 1299.99, DiscountPrice = 1199.99, StockQuantity = 15 },

                // Variants for Anti-Aging Skincare Collection (ProductId = 10)
                new ProductVariant { Id = 16, ProductId = 10, VariantName = "For Normal Skin", SKU = "BPC-AAGE-010-NORM", Price = 89.99, DiscountPrice = 75.99, StockQuantity = 15 },
                new ProductVariant { Id = 17, ProductId = 10, VariantName = "For Dry Skin", SKU = "BPC-AAGE-010-DRY", Price = 89.99, DiscountPrice = 75.99, StockQuantity = 10 },
                new ProductVariant { Id = 18, ProductId = 10, VariantName = "For Sensitive Skin", SKU = "BPC-AAGE-010-SENS", Price = 94.99, DiscountPrice = 79.99, StockQuantity = 5 },

                // Variants for Digital SLR Camera (ProductId = 6)
                new ProductVariant { Id = 19, ProductId = 6, VariantName = "Color - Black", SKU = "ELC-CAM-006-BLK", Price = 899.99, DiscountPrice = 799.99, StockQuantity = 15 },
                new ProductVariant { Id = 20, ProductId = 6, VariantName = "Color - Silver", SKU = "ELC-CAM-006-SLV", Price = 899.99, DiscountPrice = 799.99, StockQuantity = 10 },
                new ProductVariant { Id = 21, ProductId = 6, VariantName = "Kit with 18-135mm Lens", SKU = "ELC-CAM-006-135L", Price = 1099.99, DiscountPrice = 999.99, StockQuantity = 8 },

                // Variants for Organic Green Tea Gift Set (ProductId = 7)
                new ProductVariant { Id = 22, ProductId = 7, VariantName = "6 Variety Pack", SKU = "FBD-TEA-007-6VP", Price = 45.00, DiscountPrice = 45.00, StockQuantity = 20 },
                new ProductVariant { Id = 23, ProductId = 7, VariantName = "12 Variety Pack", SKU = "FBD-TEA-007-12VP", Price = 75.00, DiscountPrice = 65.00, StockQuantity = 15 },

                // Variants for Mystery Novel Collection (ProductId = 8)
                new ProductVariant { Id = 24, ProductId = 8, VariantName = "Paperback Set", SKU = "BOK-MYST-008-PB", Price = 39.99, DiscountPrice = 34.99, StockQuantity = 30 },
                new ProductVariant { Id = 25, ProductId = 8, VariantName = "Hardcover Set", SKU = "BOK-MYST-008-HC", Price = 59.99, DiscountPrice = 49.99, StockQuantity = 15 },
                new ProductVariant { Id = 26, ProductId = 8, VariantName = "E-book Collection", SKU = "BOK-MYST-008-EB", Price = 29.99, DiscountPrice = 24.99, StockQuantity = 100 },

                // Variants for Interactive Learning Robot for Kids (ProductId = 11)
                new ProductVariant { Id = 27, ProductId = 11, VariantName = "Color - Blue", SKU = "TOY-ROBOT-011-BLU", Price = 79.99, DiscountPrice = 69.99, StockQuantity = 10 },
                new ProductVariant { Id = 28, ProductId = 11, VariantName = "Color - Pink", SKU = "TOY-ROBOT-011-PNK", Price = 79.99, DiscountPrice = 69.99, StockQuantity = 8 },
                new ProductVariant { Id = 29, ProductId = 11, VariantName = "Deluxe Edition", SKU = "TOY-ROBOT-011-DLX", Price = 99.99, DiscountPrice = 89.99, StockQuantity = 5 },

                // Variants for Smartphone Stabilizer Gimbal (ProductId = 12)
                new ProductVariant { Id = 30, ProductId = 12, VariantName = "Standard Edition", SKU = "ELC-GIMB-012-STD", Price = 85.00, DiscountPrice = 69.99, StockQuantity = 25 },
                new ProductVariant { Id = 31, ProductId = 12, VariantName = "Pro Edition", SKU = "ELC-GIMB-012-PRO", Price = 120.00, DiscountPrice = 99.99, StockQuantity = 15 },

                // Variants for Smart Home Starter Kit (ProductId = 13)
                new ProductVariant { Id = 32, ProductId = 13, VariantName = "Basic Kit", SKU = "ELC-SMHM-013-BAS", Price = 179.99, DiscountPrice = 149.99, StockQuantity = 15 },
                new ProductVariant { Id = 33, ProductId = 13, VariantName = "Extended Kit", SKU = "ELC-SMHM-013-EXT", Price = 249.99, DiscountPrice = 219.99, StockQuantity = 10 },

                // Variants for Wireless Earbuds with Charging Case (ProductId = 14)
                new ProductVariant { Id = 34, ProductId = 14, VariantName = "Color - Black", SKU = "ELC-EARB-014-BLK", Price = 59.99, DiscountPrice = 49.99, StockQuantity = 30 },
                new ProductVariant { Id = 35, ProductId = 14, VariantName = "Color - White", SKU = "ELC-EARB-014-WHT", Price = 59.99, DiscountPrice = 49.99, StockQuantity = 20 },
                new ProductVariant { Id = 36, ProductId = 14, VariantName = "Color - Blue", SKU = "ELC-EARB-014-BLU", Price = 59.99, DiscountPrice = 49.99, StockQuantity = 10 },

                // Variants for Eco-Friendly Yoga Mat (ProductId = 15)
                new ProductVariant { Id = 37, ProductId = 15, VariantName = "Color - Green", SKU = "SPO-YOGA-015-GRN", Price = 39.99, DiscountPrice = 34.99, StockQuantity = 20 },
                new ProductVariant { Id = 38, ProductId = 15, VariantName = "Color - Blue", SKU = "SPO-YOGA-015-BLU", Price = 39.99, DiscountPrice = 34.99, StockQuantity = 15 },
                new ProductVariant { Id = 39, ProductId = 15, VariantName = "Thickness - 6mm", SKU = "SPO-YOGA-015-6MM", Price = 44.99, DiscountPrice = 39.99, StockQuantity = 10 },

                // Variants for Luxury Silk Tie (ProductId = 16)
                new ProductVariant { Id = 40, ProductId = 16, VariantName = "Pattern - Paisley", SKU = "APP-TIE-016-PSLY", Price = 45.00, DiscountPrice = 45.00, StockQuantity = 10 },
                new ProductVariant { Id = 41, ProductId = 16, VariantName = "Pattern - Striped", SKU = "APP-TIE-016-STR", Price = 45.00, DiscountPrice = 45.00, StockQuantity = 8 },
                new ProductVariant { Id = 42, ProductId = 16, VariantName = "Color - Navy Blue", SKU = "APP-TIE-016-NVY", Price = 45.00, DiscountPrice = 45.00, StockQuantity = 12 },

                // Variants for Pet Bed with Cooling Gel (ProductId = 17)
                new ProductVariant { Id = 43, ProductId = 17, VariantName = "Size - Small", SKU = "PET-BED-017-SML", Price = 69.99, DiscountPrice = 59.99, StockQuantity = 8 },
                new ProductVariant { Id = 44, ProductId = 17, VariantName = "Size - Medium", SKU = "PET-BED-017-MED", Price = 89.99, DiscountPrice = 79.99, StockQuantity = 7 },
                new ProductVariant { Id = 45, ProductId = 17, VariantName = "Size - Large", SKU = "PET-BED-017-LRG", Price = 109.99, DiscountPrice = 99.99, StockQuantity = 5 },

                // Variants for Gourmet Chocolate Truffle Box (ProductId = 18)
                new ProductVariant { Id = 46, ProductId = 18, VariantName = "24 Piece Box", SKU = "FBD-CHOC-018-24PC", Price = 29.99, DiscountPrice = 24.99, StockQuantity = 20 },
                new ProductVariant { Id = 47, ProductId = 18, VariantName = "48 Piece Box", SKU = "FBD-CHOC-018-48PC", Price = 49.99, DiscountPrice = 39.99, StockQuantity = 10 },

                // Variants for Office Chair (ProductId = 19)
                new ProductVariant { Id = 48, ProductId = 19, VariantName = "Color - Black", SKU = "FURN-OFCH-019-BLK", Price = 199.99, DiscountPrice = 179.99, StockQuantity = 15 },
                new ProductVariant { Id = 49, ProductId = 19, VariantName = "Color - Grey", SKU = "FURN-OFCH-019-GRY", Price = 199.99, DiscountPrice = 179.99, StockQuantity = 10 },
                new ProductVariant { Id = 50, ProductId = 19, VariantName = "With Lumbar Support", SKU = "FURN-OFCH-019-LUM", Price = 229.99, DiscountPrice = 199.99, StockQuantity = 8 },

                // Variants for Children's Storybook (ProductId = 20)
                new ProductVariant { Id = 51, ProductId = 20, VariantName = "Hardcover", SKU = "BOK-KIDS-020-HC", Price = 15.00, DiscountPrice = 12.00, StockQuantity = 30 },
                new ProductVariant { Id = 52, ProductId = 20, VariantName = "Paperback", SKU = "BOK-KIDS-020-PB", Price = 9.99, DiscountPrice = 7.99, StockQuantity = 40 },

                // Variants for Mountain Bike (ProductId = 21)
                new ProductVariant { Id = 53, ProductId = 21, VariantName = "Size - Medium (27.5\" Wheels)", SKU = "SPO-MTBIKE-021-M27", Price = 499.00, DiscountPrice = 449.00, StockQuantity = 5 },
                new ProductVariant { Id = 54, ProductId = 21, VariantName = "Size - Large (29\" Wheels)", SKU = "SPO-MTBIKE-021-L29", Price = 549.00, DiscountPrice = 499.00, StockQuantity = 4 },

                // Variants for Premium Dog Food (ProductId = 22)
                new ProductVariant { Id = 55, ProductId = 22, VariantName = "Flavor - Chicken (5kg Bag)", SKU = "PET-DGFD-022-CK5", Price = 35.00, DiscountPrice = 30.00, StockQuantity = 25 },
                new ProductVariant { Id = 56, ProductId = 22, VariantName = "Flavor - Beef (5kg Bag)", SKU = "PET-DGFD-022-BF5", Price = 35.00, DiscountPrice = 30.00, StockQuantity = 20 },
                new ProductVariant { Id = 57, ProductId = 22, VariantName = "Flavor - Lamb (10kg Bag)", SKU = "PET-DGFD-022-LB10", Price = 60.00, DiscountPrice = 50.00, StockQuantity = 15 },

                // Variants for Organic Shampoo (ProductId = 23)
                new ProductVariant { Id = 58, ProductId = 23, VariantName = "Type - Normal Hair (250ml)", SKU = "BPC-SHMP-023-NORM250", Price = 15.00, DiscountPrice = 12.99, StockQuantity = 30 },
                new ProductVariant { Id = 59, ProductId = 23, VariantName = "Type - Oily Hair (250ml)", SKU = "BPC-SHMP-023-OILY250", Price = 15.00, DiscountPrice = 12.99, StockQuantity = 25 },
                new ProductVariant { Id = 60, ProductId = 23, VariantName = "Type - Dry Hair (500ml)", SKU = "BPC-SHMP-023-DRY500", Price = 25.00, DiscountPrice = 21.99, StockQuantity = 20 }
            );

            // Seed Currencies
            modelBuilder.Entity<Currency>().HasData(
                new Currency { Id = 1, Code = "USD", Symbol = "$", Name = "US Dollar" },
                new Currency { Id = 2, Code = "EUR", Symbol = "€", Name = "Euro" },
                new Currency { Id = 3, Code = "GBP", Symbol = "£", Name = "British Pound" },
                new Currency { Id = 4, Code = "CAD", Symbol = "C$", Name = "Canadian Dollar" },
                new Currency { Id = 5, Code = "AUD", Symbol = "A$", Name = "Australian Dollar" },
                new Currency { Id = 6, Code = "JPY", Symbol = "¥", Name = "Japanese Yen" },
                new Currency { Id = 7, Code = "INR", Symbol = "₹", Name = "Indian Rupee" },
                new Currency { Id = 8, Code = "CHF", Symbol = "Fr", Name = "Swiss Franc" }
            );

            // Seed Timezones
            modelBuilder.Entity<Timezone>().HasData(
                new Timezone { Id = 1, Name = "America/New_York", UtcOffset = "-05:00", UtcOffsetString = "EST", Abbreviation = "EST" },
                new Timezone { Id = 2, Name = "Europe/London", UtcOffset = "+00:00", UtcOffsetString = "GMT", Abbreviation = "GMT" },
                new Timezone { Id = 3, Name = "Asia/Tokyo", UtcOffset = "+09:00", UtcOffsetString = "JST", Abbreviation = "JST" },
                new Timezone { Id = 4, Name = "Australia/Sydney", UtcOffset = "+10:00", UtcOffsetString = "AEDT", Abbreviation = "AEDT" },
                new Timezone { Id = 5, Name = "America/Toronto", UtcOffset = "-05:00", UtcOffsetString = "EST", Abbreviation = "EST" },
                new Timezone { Id = 6, Name = "Europe/Paris", UtcOffset = "+01:00", UtcOffsetString = "CET", Abbreviation = "CET" },
                new Timezone { Id = 7, Name = "Asia/Mumbai", UtcOffset = "+05:30", UtcOffsetString = "IST", Abbreviation = "IST" },
                new Timezone { Id = 8, Name = "Europe/Zurich", UtcOffset = "+01:00", UtcOffsetString = "CET", Abbreviation = "CET" }
            );

            // Seed Customers (8 seeds)
            modelBuilder.Entity<Customer>().HasData(
                new Customer
                {
                    Id = 1,
                    Name = "John Doe",
                    Email = "john.doe@example.com",
                    PhoneNumber = "555-0101",
                    CompanyId = 1, // Tech Solutions Inc.
                },
                new Customer
                {
                    Id = 2,
                    Name = "Jane Smith",
                    Email = "jane.smith@example.com",
                    PhoneNumber = "555-0102",
                    CompanyId = 2, // Fashion Forward Ltd.
                },
                new Customer
                {
                    Id = 3,
                    Name = "Hiroshi Tanaka",
                    Email = "hiroshi.tanaka@example.com",
                    PhoneNumber = "555-0103",
                    CompanyId = 1, // Tech Solutions Inc.
                },
                new Customer
                {
                    Id = 4,
                    Name = "Emma Brown",
                    Email = "emma.brown@example.com",
                    PhoneNumber = "555-0104",
                    CompanyId = 5, // Adventure Gear Corp.
                },
                new Customer
                {
                    Id = 5,
                    Name = "Liam Johnson",
                    Email = "liam.johnson@example.com",
                    PhoneNumber = "555-0105",
                    CompanyId = 6, // Glow & Glam
                },
                new Customer
                {
                    Id = 6,
                    Name = "Sophie Martin",
                    Email = "sophie.martin@example.com",
                    PhoneNumber = "555-0106",
                    CompanyId = 7, // Fun Time Toys
                },
                new Customer
                {
                    Id = 7,
                    Name = "Arjun Patel",
                    Email = "arjun.patel@example.com",
                    PhoneNumber = "555-0107",
                    CompanyId = 4, // Global Reads
                },
                new Customer
                {
                    Id = 8,
                    Name = "Clara Fischer",
                    Email = "clara.fischer@example.com",
                    PhoneNumber = "555-0108",
                    CompanyId = 1, // Tech Solutions Inc.
                }
            );

            // Seed Address for Customers (owned type)
            modelBuilder.Entity<Customer>()
                .OwnsOne(c => c.Address)
                .HasData(
                    new
                    {
                        CustomerId = 1,
                        Address1 = "123 Maple St",
                        Address2 = "Apt 4B",
                        City = "Springfield",
                        State = "IL",
                        Country = "USA",
                        ZipCode = "62701"
                    },
                    new
                    {
                        CustomerId = 2,
                        Address1 = "456 Oak Ave",
                        Address2 = "Suite 201",
                        City = "London",
                        State = "Greater London",
                        Country = "UK",
                        ZipCode = "SW1A 1AA"
                    },
                    new
                    {
                        CustomerId = 3,
                        Address1 = "789 Sakura St",
                        Address2 = "2 Chome-1-1",
                        City = "Tokyo",
                        State = "Tokyo",
                        Country = "Japan",
                        ZipCode = "100-0001"
                    },
                    new
                    {
                        CustomerId = 4,
                        Address1 = "101 Pine Rd",
                        Address2 = "Level 5",
                        City = "Sydney",
                        State = "NSW",
                        Country = "Australia",
                        ZipCode = "2000"
                    },
                    new
                    {
                        CustomerId = 5,
                        Address1 = "202 Birch Ln",
                        Address2 = "Unit 12",
                        City = "Toronto",
                        State = "ON",
                        Country = "Canada",
                        ZipCode = "M5V 2T7"
                    },
                    new
                    {
                        CustomerId = 6,
                        Address1 = "303 Cedar St",
                        Address2 = "Batiment C",
                        City = "Paris",
                        State = "Île-de-France",
                        Country = "France",
                        ZipCode = "75001"
                    },
                    new
                    {
                        CustomerId = 7,
                        Address1 = "404 Elm Dr",
                        Address2 = "Near Main Gate",
                        City = "Mumbai",
                        State = "MH",
                        Country = "India",
                        ZipCode = "400001"
                    },
                    new
                    {
                        CustomerId = 8,
                        Address1 = "505 Spruce Ct",
                        Address2 = "Block A",
                        City = "Zurich",
                        State = "Zurich",
                        Country = "Switzerland",
                        ZipCode = "8001"
                    }
                );

            // Seed Locations (8 seeds)
            modelBuilder.Entity<Location>().HasData(
                new Location
                {
                    Id = 1,
                    CompanyId = 1, // Tech Solutions Inc.
                    Name = "Silicon City Office",
                    CurrencyId = 1, // USD
                    TimezoneId = 1, // America/New_York
                },
                new Location
                {
                    Id = 2,
                    CompanyId = 2, // Fashion Forward Ltd.
                    Name = "Fashionville Store",
                    CurrencyId = 1, // USD
                    TimezoneId = 1, // America/New_York
                },
                new Location
                {
                    Id = 3,
                    CompanyId = 3, // Green Living Co.
                    Name = "Eco City Warehouse",
                    CurrencyId = 1, // USD
                    TimezoneId = 1, // America/New_York
                },
                new Location
                {
                    Id = 4,
                    CompanyId = 4, // Global Reads
                    Name = "London Bookstore",
                    CurrencyId = 3, // GBP
                    TimezoneId = 2, // Europe/London
                },
                new Location
                {
                    Id = 5,
                    CompanyId = 5, // Adventure Gear Corp.
                    Name = "Sydney Outlet",
                    CurrencyId = 5, // AUD
                    TimezoneId = 4, // Australia/Sydney
                },
                new Location
                {
                    Id = 6,
                    CompanyId = 6, // Glow & Glam
                    Name = "Paris Boutique",
                    CurrencyId = 2, // EUR
                    TimezoneId = 6, // Europe/Paris
                },
                new Location
                {
                    Id = 7,
                    CompanyId = 7, // Fun Time Toys
                    Name = "Mumbai Store",
                    CurrencyId = 7, // INR
                    TimezoneId = 7, // Asia/Mumbai
                },
                new Location
                {
                    Id = 8,
                    CompanyId = 1, // Tech Solutions Inc.
                    Name = "Zurich Tech Hub",
                    CurrencyId = 8, // CHF
                    TimezoneId = 8, // Europe/Zurich
                }
            );

            // Seed Address for Locations (owned type)
            modelBuilder.Entity<Location>()
                .OwnsOne(l => l.Address)
                .HasData(
                    new
                    {
                        LocationId = 1,
                        Address1 = "123 Innovation Way",
                        Address2 = "Tech Park, Suite 100",
                        City = "Silicon City",
                        State = "CA",
                        Country = "USA",
                        ZipCode = "94016"
                    },
                    new
                    {
                        LocationId = 2,
                        Address1 = "456 Style Avenue",
                        Address2 = "Fashion Mall, Unit 22",
                        City = "Fashionville",
                        State = "NY",
                        Country = "USA",
                        ZipCode = "10001"
                    },
                    new
                    {
                        LocationId = 3,
                        Address1 = "789 Earth Street",
                        Address2 = "Industrial Zone, Gate 5",
                        City = "Eco City",
                        State = "GA",
                        Country = "USA",
                        ZipCode = "30303"
                    },
                    new
                    {
                        LocationId = 4,
                        Address1 = "101 Literary Lane",
                        Address2 = "Off Charing Cross Rd",
                        City = "London",
                        State = "London",
                        Country = "UK",
                        ZipCode = "WC1B 3PA"
                    },
                    new
                    {
                        LocationId = 5,
                        Address1 = "222 Trail Road",
                        Address2 = "Near Blue Mountains Entry",
                        City = "Sydney",
                        State = "NSW",
                        Country = "Australia",
                        ZipCode = "2000"
                    },
                    new
                    {
                        LocationId = 6,
                        Address1 = "333 Radiant Road",
                        Address2 = "Galerie Vivienne",
                        City = "Paris",
                        State = "Paris",
                        Country = "France",
                        ZipCode = "75002"
                    },
                    new
                    {
                        LocationId = 7,
                        Address1 = "444 Playful Place",
                        Address2 = "Linking Road, Bandra",
                        City = "Mumbai",
                        State = "MH",
                        Country = "India",
                        ZipCode = "400002"
                    },
                    new
                    {
                        LocationId = 8,
                        Address1 = "555 Tech Park",
                        Address2 = "Innovation Center, Floor 3",
                        City = "Zurich",
                        State = "",
                        Country = "Switzerland",
                        ZipCode = "8002"
                    }
                );

            // Seed OrderHeaders (to link with Invoices)
            modelBuilder.Entity<OrderHeader>().HasData(
                new OrderHeader
                {
                    Id = 1,
                    ApplicationUserId = null,
                    CustomerId = 1,
                    OrderDate = new DateTime(2025, 4, 1),
                    ShippingDate = new DateTime(2025, 4, 3),
                    EstimatedDelivery = new DateTime(2025, 4, 5),
                    Subtotal = 599.99m,
                    Tax = 48.00m,
                    Discount = 13.00m,
                    OrderTotal = 649.99m,
                    AmountPaid = 649.99m,
                    AmountDue = 0.00m,
                    ShippingCharges = 15.00m,
                    OrderStatus = "Shipped",
                    PaymentStatus = "Paid",
                    DeliveryStatus = "InTransit",
                    ShippingMethod = "Standard",
                    DeliveryMethod = "Ground",
                    TrackingNumber = "TRK123456",
                    Carrier = "UPS",
                    PaymentDate = new DateTime(2025, 4, 1),
                    PaymentDueDate = new DateOnly(2025, 4, 30),
                    ShippingContactPhone = "555-0101",
                    ShippingContactName = "John Doe",
                    PaymentMethod = "CreditCard",
                    CustomerNotes = "Deliver to front porch",
                },
                new OrderHeader
                {
                    Id = 2,
                    ApplicationUserId = null,
                    CustomerId = 2,
                    OrderDate = new DateTime(2025, 4, 2),
                    ShippingDate = null,
                    EstimatedDelivery = new DateTime(2025, 4, 7),
                    Subtotal = 119.99m,
                    Tax = 9.60m,
                    Discount = 0.00m,
                    OrderTotal = 129.59m,
                    AmountPaid = 0.00m,
                    AmountDue = 129.59m,
                    ShippingCharges = 0.00m,
                    OrderStatus = "Processing",
                    PaymentStatus = "Pending",
                    DeliveryStatus = "Pending",
                    ShippingMethod = "Express",
                    DeliveryMethod = "Air",
                    TrackingNumber = null,
                    Carrier = null,
                    PaymentDate = null,
                    PaymentDueDate = new DateOnly(2025, 5, 2),
                    ShippingContactPhone = "555-0102",
                    ShippingContactName = "Jane Smith",
                    PaymentMethod = "PayPal",
                    CustomerNotes = null,
                },
                new OrderHeader
                {
                    Id = 3,
                    ApplicationUserId = null,
                    CustomerId = 2,
                    OrderDate = new DateTime(2025, 4, 3),
                    ShippingDate = new DateTime(2025, 4, 5),
                    EstimatedDelivery = new DateTime(2025, 4, 7),
                    Subtotal = 735.99m,
                    Tax = 58.88m,
                    Discount = 14.88m,
                    OrderTotal = 799.99m,
                    AmountPaid = 799.99m,
                    AmountDue = 0.00m,
                    ShippingCharges = 20.00m,
                    OrderStatus = "Delivered",
                    PaymentStatus = "Paid",
                    DeliveryStatus = "Delivered",
                    ShippingMethod = "Standard",
                    DeliveryMethod = "Ground",
                    TrackingNumber = "TRK789012",
                    Carrier = "FedEx",
                    PaymentDate = new DateTime(2025, 4, 3),
                    PaymentDueDate = new DateOnly(2025, 5, 3),
                    ShippingContactPhone = "555-0103",
                    ShippingContactName = "Hiroshi Tanaka",
                    PaymentMethod = "CreditCard",
                    CustomerNotes = "Leave at reception",
                },
                new OrderHeader
                {
                    Id = 4,
                    ApplicationUserId = null,
                    CustomerId = 3,
                    OrderDate = new DateTime(2025, 4, 4),
                    ShippingDate = new DateTime(2025, 4, 6),
                    EstimatedDelivery = new DateTime(2025, 4, 8),
                    Subtotal = 100.00m,
                    Tax = 8.00m,
                    Discount = 0.00m,
                    OrderTotal = 108.00m,
                    AmountPaid = 108.00m,
                    AmountDue = 0.00m,
                    ShippingCharges = 0.00m,
                    OrderStatus = "Shipped",
                    PaymentStatus = "Paid",
                    DeliveryStatus = "InTransit",
                    ShippingMethod = "Express",
                    DeliveryMethod = "Air",
                    TrackingNumber = "TRK345678",
                    Carrier = "DHL",
                    PaymentDate = new DateTime(2025, 4, 4),
                    PaymentDueDate = new DateOnly(2025, 5, 4),
                    ShippingContactPhone = "555-0104",
                    ShippingContactName = "Emma Brown",
                    PaymentMethod = "DebitCard",
                    CustomerNotes = null,
                },
                new OrderHeader
                {
                    Id = 5,
                    ApplicationUserId = null,
                    CustomerId = 4,
                    OrderDate = new DateTime(2025, 4, 5),
                    ShippingDate = null,
                    EstimatedDelivery = new DateTime(2025, 4, 10),
                    Subtotal = 70.00m,
                    Tax = 5.60m,
                    Discount = 0.00m,
                    OrderTotal = 75.60m,
                    AmountPaid = 0.00m,
                    AmountDue = 75.60m,
                    ShippingCharges = 0.00m,
                    OrderStatus = "Processing",
                    PaymentStatus = "Pending",
                    DeliveryStatus = "Pending",
                    ShippingMethod = "Standard",
                    DeliveryMethod = "Ground",
                    TrackingNumber = null,
                    Carrier = null,
                    PaymentDate = null,
                    PaymentDueDate = new DateOnly(2025, 5, 5),
                    ShippingContactPhone = "555-0105",
                    ShippingContactName = "Liam Johnson",
                    PaymentMethod = "PayPal",
                    CustomerNotes = "Fragile items",
                },
                new OrderHeader
                {
                    Id = 6,
                    ApplicationUserId = null,
                    CustomerId = 4,
                    OrderDate = new DateTime(2025, 4, 6),
                    ShippingDate = new DateTime(2025, 4, 8),
                    EstimatedDelivery = new DateTime(2025, 4, 10),
                    Subtotal = 64.99m,
                    Tax = 5.20m,
                    Discount = 0.00m,
                    OrderTotal = 70.19m,
                    AmountPaid = 70.19m,
                    AmountDue = 0.00m,
                    ShippingCharges = 0.00m,
                    OrderStatus = "Shipped",
                    PaymentStatus = "Paid",
                    DeliveryStatus = "InTransit",
                    ShippingMethod = "Standard",
                    DeliveryMethod = "Ground",
                    TrackingNumber = "TRK901234",
                    Carrier = "UPS",
                    PaymentDate = new DateTime(2025, 4, 6),
                    PaymentDueDate = new DateOnly(2025, 5, 6),
                    ShippingContactPhone = "555-0106",
                    ShippingContactName = "Sophie Martin",
                    PaymentMethod = "CreditCard",
                    CustomerNotes = null,
                },
                new OrderHeader
                {
                    Id = 7,
                    ApplicationUserId = null,
                    CustomerId = 4,
                    OrderDate = new DateTime(2025, 4, 7),
                    ShippingDate = new DateTime(2025, 4, 9),
                    EstimatedDelivery = new DateTime(2025, 4, 11),
                    Subtotal = 14.99m,
                    Tax = 1.20m,
                    Discount = 0.00m,
                    OrderTotal = 16.19m,
                    AmountPaid = 16.19m,
                    AmountDue = 0.00m,
                    ShippingCharges = 0.00m,
                    OrderStatus = "Delivered",
                    PaymentStatus = "Paid",
                    DeliveryStatus = "Delivered",
                    ShippingMethod = "Express",
                    DeliveryMethod = "Air",
                    TrackingNumber = "TRK567890",
                    Carrier = "FedEx",
                    PaymentDate = new DateTime(2025, 4, 7),
                    PaymentDueDate = new DateOnly(2025, 5, 7),
                    ShippingContactPhone = "555-0107",
                    ShippingContactName = "Arjun Patel",
                    PaymentMethod = "DebitCard",
                    CustomerNotes = "Urgent delivery",
                },
                new OrderHeader
                {
                    Id = 8,
                    ApplicationUserId = null,
                    CustomerId = 5,
                    OrderDate = new DateTime(2025, 4, 8),
                    ShippingDate = null,
                    EstimatedDelivery = new DateTime(2025, 4, 13),
                    Subtotal = 139.99m,
                    Tax = 11.20m,
                    Discount = 0.00m,
                    OrderTotal = 151.19m,
                    AmountPaid = 0.00m,
                    AmountDue = 151.19m,
                    ShippingCharges = 0.00m,
                    OrderStatus = "Processing",
                    PaymentStatus = "Pending",
                    DeliveryStatus = "Pending",
                    ShippingMethod = "Standard",
                    DeliveryMethod = "Ground",
                    TrackingNumber = null,
                    Carrier = null,
                    PaymentDate = null,
                    PaymentDueDate = new DateOnly(2025, 5, 8),
                    ShippingContactPhone = "555-0108",
                    ShippingContactName = "Clara Fischer",
                    PaymentMethod = "CreditCard",
                    CustomerNotes = null,
                },
                  new OrderHeader
                  {
                      Id = 9,
                      ApplicationUserId = null,
                      CustomerId = 6,
                      OrderDate = new DateTime(2025, 4, 8),
                      ShippingDate = null,
                      EstimatedDelivery = new DateTime(2025, 4, 13),
                      Subtotal = 139.99m,
                      Tax = 11.20m,
                      Discount = 0.00m,
                      OrderTotal = 151.19m,
                      AmountPaid = 0.00m,
                      AmountDue = 151.19m,
                      ShippingCharges = 0.00m,
                      OrderStatus = "Processing",
                      PaymentStatus = "Pending",
                      DeliveryStatus = "Pending",
                      ShippingMethod = "Standard",
                      DeliveryMethod = "Ground",
                      TrackingNumber = null,
                      Carrier = null,
                      PaymentDate = null,
                      PaymentDueDate = new DateOnly(2025, 5, 8),
                      ShippingContactPhone = "555-0109",
                      ShippingContactName = "Clara Fischer",
                      PaymentMethod = "PayPal",
                      CustomerNotes = "Gift wrap required",
                  }
            );

            // Seed Shipping Address for OrderHeaders (owned type)
            modelBuilder.Entity<OrderHeader>()
             .OwnsOne(l => l.ShipToAddress)
             .HasData(
                 new
                 {
                     OrderHeaderId = 1,
                     ShippingAddress1 = "123 Maple St",
                     ShippingAddress2 = "Suite 100",
                     ShippingCity = "Springfield",
                     ShippingState = "IL",
                     ShippingCountry = "USA",
                     ShippingZipCode = "94016"
                 },
                 new
                 {
                     OrderHeaderId = 2,
                     ShippingAddress1 = "456 Style Avenue",
                     ShippingAddress2 = "Oak Ave",
                     ShippingCity = "London",
                     ShippingState = "NY",
                     ShippingCountry = "USA",
                     ShippingZipCode = "10001"
                 },
                 new
                 {
                     OrderHeaderId = 3,
                     ShippingAddress1 = "789 Earth Street",
                     ShippingAddress2 = "Industrial Zone Ave ",
                     ShippingCity = "Eco City",
                     ShippingState = "GA",
                     ShippingCountry = "USA",
                     ShippingZipCode = "30303"
                 },
                 new
                 {
                     OrderHeaderId = 4,
                     ShippingAddress1 = "101 Literary Lane",
                     ShippingAddress2 = "Off Charing Cross Rd",
                     ShippingCity = "London",
                     ShippingState = "London",
                     ShippingCountry = "UK",
                     ShippingZipCode = "WC1B 3PA"
                 },
                 new
                 {
                     OrderHeaderId = 5,
                     ShippingAddress1 = "222 Trail Road",
                     ShippingAddress2 = "Near Blue Mountains Entry",
                     ShippingCity = "Sydney",
                     ShippingState = "NSW",
                     ShippingCountry = "Australia",
                     ShippingZipCode = "2000"
                 },
                 new
                 {
                     OrderHeaderId = 6,
                     ShippingAddress1 = "333 Radiant Road",
                     ShippingAddress2 = "Galerie Vivienne",
                     ShippingCity = "Paris",
                     ShippingState = "Paris",
                     ShippingCountry = "France",
                     ShippingZipCode = "75002"
                 },
                 new
                 {
                     OrderHeaderId = 7,
                     ShippingAddress1 = "444 Playful Place",
                     ShippingAddress2 = "Linking Road, Bandra",
                     ShippingCity = "Mumbai",
                     ShippingState = "MH",
                     ShippingCountry = "India",
                     ShippingZipCode = "400002"
                 },
                 new
                 {
                     OrderHeaderId = 8,
                     ShippingAddress1 = "555 Tech Park",
                     ShippingAddress2 = "Innovation Center, Floor 3",
                     ShippingCity = "Zurich",
                     ShippingState = "",
                     ShippingCountry = "Switzerland",
                     ShippingZipCode = "8002"
                 },
                 new
                 {
                     OrderHeaderId = 9,
                     ShippingAddress1 = "777 Skyline Boulevard",
                     ShippingAddress2 = "Sky Tower, Apt 905",
                     ShippingCity = "Toronto",
                     ShippingState = "ON",
                     ShippingCountry = "Canada",
                     ShippingZipCode = "M5V 2T6"
                 }
             );

            // Seed Billing Address for OrderHeaders (owned type)
            modelBuilder.Entity<OrderHeader>()
             .OwnsOne(l => l.BillToAddress)
             .HasData(
                 new
                 {
                     OrderHeaderId = 1,
                     BillingAddress1 = "123 Maple St",
                     BillingAddress2 = "Suite 100",
                     BillingCity = "Springfield",
                     BillingState = "IL",
                     BillingCountry = "USA",
                     BillingZipCode = "94016"
                 },
                 new
                 {
                     OrderHeaderId = 2,
                     BillingAddress1 = "456 Style Avenue",
                     BillingAddress2 = "Oak Ave",
                     BillingCity = "London",
                     BillingState = "NY",
                     BillingCountry = "USA",
                     BillingZipCode = "10001"
                 },
                 new
                 {
                     OrderHeaderId = 3,
                     BillingAddress1 = "789 Earth Street",
                     BillingAddress2 = "Industrial Zone Ave ",
                     BillingCity = "Eco City",
                     BillingState = "GA",
                     BillingCountry = "USA",
                     BillingZipCode = "30303"
                 },
                 new
                 {
                     OrderHeaderId = 4,
                     BillingAddress1 = "101 Literary Lane",
                     BillingAddress2 = "Off Charing Cross Rd",
                     BillingCity = "London",
                     BillingState = "London",
                     BillingCountry = "UK",
                     BillingZipCode = "WC1B 3PA"
                 },
                 new
                 {
                     OrderHeaderId = 5,
                     BillingAddress1 = "222 Trail Road",
                     BillingAddress2 = "Near Blue Mountains Entry",
                     BillingCity = "Sydney",
                     BillingState = "NSW",
                     BillingCountry = "Australia",
                     BillingZipCode = "2000"
                 },
                 new
                 {
                     OrderHeaderId = 6,
                     BillingAddress1 = "333 Radiant Road",
                     BillingAddress2 = "Galerie Vivienne",
                     BillingCity = "Paris",
                     BillingState = "Paris",
                     BillingCountry = "France",
                     BillingZipCode = "75002"
                 },
                 new
                 {
                     OrderHeaderId = 7,
                     BillingAddress1 = "444 Playful Place",
                     BillingAddress2 = "Linking Road, Bandra",
                     BillingCity = "Mumbai",
                     BillingState = "MH",
                     BillingCountry = "India",
                     BillingZipCode = "400002"
                 },
                 new
                 {
                     OrderHeaderId = 8,
                     BillingAddress1 = "555 Tech Park",
                     BillingAddress2 = "Innovation Center, Floor 3",
                     BillingCity = "Zurich",
                     BillingState = "",
                     BillingCountry = "Switzerland",
                     BillingZipCode = "8002"
                 },
                 new
                 {
                     OrderHeaderId = 9,
                     BillingAddress1 = "777 Skyline Boulevard",
                     BillingAddress2 = "Sky Tower, Apt 905",
                     BillingCity = "Toronto",
                     BillingState = "ON",
                     BillingCountry = "Canada",
                     BillingZipCode = "M5V 2T6"
                 }
             );

            // Seed OrderDetails (10 seeds)
            modelBuilder.Entity<OrderDetail>().HasData(
                new OrderDetail
                {
                    Id = 1,
                    OrderHeaderId = 1, // John Doe's order
                    ProductId = 1, // Smart TV 55 inch
                    Count = 1,
                    Price = 649.99, // DiscountPrice from Product
                },
                new OrderDetail
                {
                    Id = 2,
                    OrderHeaderId = 2, // Jane Smith's order
                    ProductId = 5, // Leather Handbag
                    Count = 1,
                    Price = 129.99, // DiscountPrice
                },
                new OrderDetail
                {
                    Id = 3,
                    OrderHeaderId = 3, // Hiroshi Tanaka's order
                    ProductId = 6, // DSLR Camera
                    Count = 1,
                    Price = 799.99, // DiscountPrice
                },
                new OrderDetail
                {
                    Id = 4,
                    OrderHeaderId = 4, // Emma Brown's order
                    ProductId = 9, // Hiking Boots
                    Count = 1,
                    Price = 109.95, // DiscountPrice
                },
                new OrderDetail
                {
                    Id = 5,
                    OrderHeaderId = 5, // Liam Johnson's order
                    ProductId = 10, // Skincare Set
                    Count = 1,
                    Price = 75.99, // DiscountPrice
                },
                new OrderDetail
                {
                    Id = 6,
                    OrderHeaderId = 6, // Sophie Martin's order
                    ProductId = 11, // Learning Robot
                    Count = 1,
                    Price = 69.99, // DiscountPrice
                },
                new OrderDetail
                {
                    Id = 7,
                    OrderHeaderId = 7, // Arjun Patel's order
                    ProductId = 8, // Historical Fiction Book
                    Count = 1,
                    Price = 15.99, // DiscountPrice
                },
                new OrderDetail
                {
                    Id = 8,
                    OrderHeaderId = 8, // Clara Fischer's order
                    ProductId = 13, // Smart Home Kit
                    Count = 1,
                    Price = 149.99, // DiscountPrice

                },
                new OrderDetail
                {
                    Id = 9,
                    OrderHeaderId = 9, // Clara Fischer's order
                    ProductId = 13, // Smart Home Kit
                    Count = 1,
                    Price = 149.99, // DiscountPrice

                },
                new OrderDetail
                {
                    Id = 10,
                    OrderHeaderId = 9, // John Doe's order (additional item)
                    ProductId = 12, // Smartphone Gimbal
                    Count = 1,
                    Price = 69.99, // DiscountPrice
                },
                new OrderDetail
                {
                    Id = 11,
                    OrderHeaderId = 3, // Hiroshi Tanaka's order (additional item)
                    ProductId = 4, // Ultra-Slim Laptop
                    Count = 1,
                    Price = 899.99, // DiscountPrice
                }
            );

            // Seed OrderActivityLogs
            modelBuilder.Entity<OrderActivityLog>().HasData(
                new OrderActivityLog
                {
                    Id = 1,
                    OrderHeaderId = 1,
                    Timestamp = new DateTime(2025, 4, 1, 10, 0, 0),
                    User = "john.doe",
                    ActivityType = ActivityType.OrderCreated,
                    Description = "Order placed by customer",
                    Details = "{\"CustomerId\": 1}"
                },
                new OrderActivityLog
                {
                    Id = 2,
                    OrderHeaderId = 1,
                    Timestamp = new DateTime(2025, 4, 1, 10, 5, 0),
                    User = "john.doe",
                    ActivityType = ActivityType.PaymentProcessed,
                    Description = "Payment completed via CreditCard",
                    Details = "{\"Amount\": 649.99}"
                },
                new OrderActivityLog
                {
                    Id = 3,
                    OrderHeaderId = 1,
                    Timestamp = new DateTime(2025, 4, 3, 9, 0, 0),
                    User = "system",
                    ActivityType = ActivityType.ShippingUpdated,
                    Description = "Order shipped via UPS",
                    Details = "{\"TrackingNumber\": \"TRK123456\"}"
                },
                new OrderActivityLog
                {
                    Id = 4,
                    OrderHeaderId = 2,
                    Timestamp = new DateTime(2025, 4, 2, 11, 0, 0),
                    User = "jane.smith",
                    ActivityType = ActivityType.OrderCreated,
                    Description = "Order placed by customer",
                    Details = "{\"CustomerId\": 2}"
                },
                new OrderActivityLog
                {
                    Id = 5,
                    OrderHeaderId = 3,
                    Timestamp = new DateTime(2025, 4, 3, 12, 0, 0),
                    User = "hiroshi.tanaka",
                    ActivityType = ActivityType.OrderCreated,
                    Description = "Order placed by customer",
                    Details = "{\"CustomerId\": 2}"
                },
                new OrderActivityLog
                {
                    Id = 6,
                    OrderHeaderId = 3,
                    Timestamp = new DateTime(2025, 4, 3, 12, 10, 0),
                    User = "hiroshi.tanaka",
                    ActivityType = ActivityType.PaymentProcessed,
                    Description = "Payment completed via CreditCard",
                    Details = "{\"Amount\": 799.99}"
                },
                new OrderActivityLog
                {
                    Id = 7,
                    OrderHeaderId = 3,
                    Timestamp = new DateTime(2025, 4, 5, 8, 0, 0),
                    User = "system",
                    ActivityType = ActivityType.ShippingUpdated,
                    Description = "Order shipped via FedEx",
                    Details = "{\"TrackingNumber\": \"TRK789012\"}"
                },
                new OrderActivityLog
                {
                    Id = 8,
                    OrderHeaderId = 4,
                    Timestamp = new DateTime(2025, 4, 4, 14, 0, 0),
                    User = "emma.brown",
                    ActivityType = ActivityType.OrderCreated,
                    Description = "Order placed by customer",
                    Details = "{\"CustomerId\": 3}"
                },
                new OrderActivityLog
                {
                    Id = 9,
                    OrderHeaderId = 4,
                    Timestamp = new DateTime(2025, 4, 4, 14, 5, 0),
                    User = "emma.brown",
                    ActivityType = ActivityType.PaymentProcessed,
                    Description = "Payment completed via DebitCard",
                    Details = "{\"Amount\": 109.95}"
                },
                new OrderActivityLog
                {
                    Id = 10,
                    OrderHeaderId = 4,
                    Timestamp = new DateTime(2025, 4, 6, 10, 0, 0),
                    User = "system",
                    ActivityType = ActivityType.ShippingUpdated,
                    Description = "Order shipped via DHL",
                    Details = "{\"TrackingNumber\": \"TRK345678\"}"
                },
                new OrderActivityLog
                {
                    Id = 11,
                    OrderHeaderId = 5,
                    Timestamp = new DateTime(2025, 4, 5, 15, 0, 0),
                    User = "liam.johnson",
                    ActivityType = ActivityType.OrderCreated,
                    Description = "Order placed by customer",
                    Details = "{\"CustomerId\": 4}"
                },
                new OrderActivityLog
                {
                    Id = 12,
                    OrderHeaderId = 6,
                    Timestamp = new DateTime(2025, 4, 6, 16, 0, 0),
                    User = "sophie.martin",
                    ActivityType = ActivityType.OrderCreated,
                    Description = "Order placed by customer",
                    Details = "{\"CustomerId\": 4}"
                },
                new OrderActivityLog
                {
                    Id = 13,
                    OrderHeaderId = 6,
                    Timestamp = new DateTime(2025, 4, 6, 16, 5, 0),
                    User = "sophie.martin",
                    ActivityType = ActivityType.PaymentProcessed,
                    Description = "Payment completed via CreditCard",
                    Details = "{\"Amount\": 69.99}"
                },
                new OrderActivityLog
                {
                    Id = 14,
                    OrderHeaderId = 6,
                    Timestamp = new DateTime(2025, 4, 8, 11, 0, 0),
                    User = "system",
                    ActivityType = ActivityType.ShippingUpdated,
                    Description = "Order shipped via UPS",
                    Details = "{\"TrackingNumber\": \"TRK901234\"}"
                },
                new OrderActivityLog
                {
                    Id = 15,
                    OrderHeaderId = 7,
                    Timestamp = new DateTime(2025, 4, 7, 17, 0, 0),
                    User = "arjun.patel",
                    ActivityType = ActivityType.OrderCreated,
                    Description = "Order placed by customer",
                    Details = "{\"CustomerId\": 4}"
                },
                new OrderActivityLog
                {
                    Id = 16,
                    OrderHeaderId = 7,
                    Timestamp = new DateTime(2025, 4, 7, 17, 5, 0),
                    User = "arjun.patel",
                    ActivityType = ActivityType.PaymentProcessed,
                    Description = "Payment completed via DebitCard",
                    Details = "{\"Amount\": 15.99}"
                },
                new OrderActivityLog
                {
                    Id = 17,
                    OrderHeaderId = 7,
                    Timestamp = new DateTime(2025, 4, 9, 9, 0, 0),
                    User = "system",
                    ActivityType = ActivityType.ShippingUpdated,
                    Description = "Order shipped via FedEx",
                    Details = "{\"TrackingNumber\": \"TRK567890\"}"
                },
                new OrderActivityLog
                {
                    Id = 18,
                    OrderHeaderId = 8,
                    Timestamp = new DateTime(2025, 4, 8, 18, 0, 0),
                    User = "clara.fischer",
                    ActivityType = ActivityType.OrderCreated,
                    Description = "Order placed by customer",
                    Details = "{\"CustomerId\": 5}"
                },
                new OrderActivityLog
                {
                    Id = 19,
                    OrderHeaderId = 9,
                    Timestamp = new DateTime(2025, 4, 8, 19, 0, 0),
                    User = "clara.fischer",
                    ActivityType = ActivityType.OrderCreated,
                    Description = "Order placed by customer",
                    Details = "{\"CustomerId\": 6}"
                }
            );

            // Seed Invoices
            modelBuilder.Entity<Invoice>().HasData(
                new Invoice
                {
                    Id = 1,
                    InvoiceNumber = "INV-2025-001",
                    PONumber = "PO-001",
                    IssueDate = new DateTime(2025, 4, 1),
                    PaymentDue = new DateTime(2025, 4, 30),
                    Status = InvoiceStatus.Paid,
                    InvoiceType = InvoiceType.Standard,
                    Notes = "Thank you for your purchase!",
                    PaymentTerms = "Net 30",
                    PaymentMethod = "Credit Card",
                    CustomerId = 1, // John Doe
                    CompanyId = 1, // Tech Solutions Inc.
                    LocationId = 1, // Silicon City Office
                    OrderId = 1, // Links to OrderHeader
                    Subtotal = 649.99m,
                    Discount = 0m,
                    Tax = 52.00m,
                    ShippingAmount = 20.00m,
                    PaidAmount = 721.99m,
                    TotalAmount = 721.99m,
                    ExternalReference = "REF-001"
                },
                new Invoice
                {
                    Id = 2,
                    InvoiceNumber = "INV-2025-002",
                    PONumber = "PO-002",
                    IssueDate = new DateTime(2025, 4, 2),
                    PaymentDue = new DateTime(2025, 5, 2),
                    Status = InvoiceStatus.Sent,
                    InvoiceType = InvoiceType.Standard,
                    Notes = "Please pay by due date.",
                    PaymentTerms = "Net 30",
                    PaymentMethod = "Bank Transfer",
                    CustomerId = 2, // Jane Smith
                    CompanyId = 2, // Fashion Forward Ltd.
                    LocationId = 2, // Fashionville Store
                    OrderId = 2,
                    Subtotal = 129.99m,
                    Discount = 10.00m,
                    Tax = 10.40m,
                    ShippingAmount = 15.00m,
                    PaidAmount = 0m,
                    TotalAmount = 145.39m,
                    ExternalReference = "REF-002"
                },
                new Invoice
                {
                    Id = 3,
                    InvoiceNumber = "INV-2025-003",
                    PONumber = "PO-003",
                    IssueDate = new DateTime(2025, 4, 3),
                    PaymentDue = new DateTime(2025, 5, 3),
                    Status = InvoiceStatus.Paid,
                    InvoiceType = InvoiceType.Proforma,
                    Notes = "Proforma invoice for approval.",
                    PaymentTerms = "Net 30",
                    PaymentMethod = "Credit Card",
                    CustomerId = 3, // Hiroshi Tanaka
                    CompanyId = 1, // Tech Solutions Inc.
                    LocationId = 1, // Silicon City Office
                    OrderId = 3,
                    Subtotal = 799.99m,
                    Discount = 0m,
                    Tax = 64.00m,
                    ShippingAmount = 25.00m,
                    PaidAmount = 888.99m,
                    TotalAmount = 888.99m,
                    ExternalReference = "REF-003"
                },
                new Invoice
                {
                    Id = 4,
                    InvoiceNumber = "INV-2025-004",
                    PONumber = "PO-004",
                    IssueDate = new DateTime(2025, 4, 4),
                    PaymentDue = new DateTime(2025, 5, 4),
                    Status = InvoiceStatus.PartiallyPaid,
                    InvoiceType = InvoiceType.Standard,
                    Notes = "Partial payment received.",
                    PaymentTerms = "Net 30",
                    PaymentMethod = "Bank Transfer",
                    CustomerId = 4, // Emma Brown
                    CompanyId = 5, // Adventure Gear Corp.
                    LocationId = 5, // Sydney Outlet
                    OrderId = 4,
                    Subtotal = 109.95m,
                    Discount = 0m,
                    Tax = 8.80m,
                    ShippingAmount = 10.00m,
                    PaidAmount = 50.00m,
                    TotalAmount = 128.75m,
                    ExternalReference = "REF-004"
                },
                new Invoice
                {
                    Id = 5,
                    InvoiceNumber = "INV-2025-005",
                    PONumber = "PO-005",
                    IssueDate = new DateTime(2025, 4, 5),
                    PaymentDue = new DateTime(2025, 5, 5),
                    Status = InvoiceStatus.Draft,
                    InvoiceType = InvoiceType.Recurring,
                    Notes = "Recurring invoice for monthly subscription.",
                    PaymentTerms = "Net 30",
                    PaymentMethod = "Direct Debit",
                    CustomerId = 5, // Liam Johnson
                    CompanyId = 6, // Glow & Glam
                    LocationId = 6, // Paris Boutique
                    OrderId = 5,
                    Subtotal = 75.99m,
                    Discount = 0m,
                    Tax = 6.08m,
                    ShippingAmount = 0m,
                    PaidAmount = 0m,
                    TotalAmount = 82.07m,
                    ExternalReference = "REF-005"
                },
                new Invoice
                {
                    Id = 6,
                    InvoiceNumber = "INV-2025-006",
                    PONumber = "PO-006",
                    IssueDate = new DateTime(2025, 4, 6),
                    PaymentDue = new DateTime(2025, 5, 6),
                    Status = InvoiceStatus.Overdue,
                    InvoiceType = InvoiceType.Standard,
                    Notes = "Payment overdue, please settle ASAP.",
                    PaymentTerms = "Net 30",
                    PaymentMethod = "Credit Card",
                    CustomerId = 6, // Sophie Martin
                    CompanyId = 7, // Fun Time Toys
                    LocationId = 7, // Mumbai Store
                    OrderId = 6,
                    Subtotal = 69.99m,
                    Discount = 0m,
                    Tax = 5.60m,
                    ShippingAmount = 8.00m,
                    PaidAmount = 0m,
                    TotalAmount = 83.59m,
                    ExternalReference = "REF-006"
                },
                new Invoice
                {
                    Id = 7,
                    InvoiceNumber = "INV-2025-007",
                    PONumber = "PO-007",
                    IssueDate = new DateTime(2025, 4, 7),
                    PaymentDue = new DateTime(2025, 5, 7),
                    Status = InvoiceStatus.Paid,
                    InvoiceType = InvoiceType.CreditNote,
                    Notes = "Credit note for returned item.",
                    PaymentTerms = "Immediate",
                    PaymentMethod = "Refund",
                    CustomerId = 7, // Arjun Patel
                    CompanyId = 4, // Global Reads
                    LocationId = 4, // London Bookstore
                    OrderId = 7,
                    Subtotal = -15.99m,
                    Discount = 0m,
                    Tax = -1.28m,
                    ShippingAmount = 0m,
                    PaidAmount = -17.27m,
                    TotalAmount = -17.27m,
                    ExternalReference = "REF-007"
                },
                new Invoice
                {
                    Id = 8,
                    InvoiceNumber = "INV-2025-008",
                    PONumber = "PO-008",
                    IssueDate = new DateTime(2025, 4, 8),
                    PaymentDue = new DateTime(2025, 5, 8),
                    Status = InvoiceStatus.Sent,
                    InvoiceType = InvoiceType.Standard,
                    Notes = "Invoice for recent purchase.",
                    PaymentTerms = "Net 30",
                    PaymentMethod = "Bank Transfer",
                    CustomerId = 8, // Clara Fischer
                    CompanyId = 1, // Tech Solutions Inc.
                    LocationId = 8, // Zurich Tech Hub
                    OrderId = 8,
                    Subtotal = 149.99m,
                    Discount = 0m,
                    Tax = 12.00m,
                    ShippingAmount = 15.00m,
                    PaidAmount = 0m,
                    TotalAmount = 176.99m,
                    ExternalReference = "REF-008"
                }
            );

            // Seed InvoiceItems (10 seeds, distributed across Invoices)
            modelBuilder.Entity<InvoiceItem>().HasData(
                new InvoiceItem { Id = 1, InvoiceId = 1, Service = "Smart TV 55 inch", Description = "55-inch 4K Smart TV with HDR", Unit = "Unit", Price = 649.99m, Amount = 649.99m },
                new InvoiceItem { Id = 2, InvoiceId = 2, Service = "Leather Handbag", Description = "Premium black leather handbag", Unit = "Unit", Price = 129.99m, Amount = 129.99m },
                new InvoiceItem { Id = 3, InvoiceId = 3, Service = "DSLR Camera", Description = "24.1MP DSLR camera with 18-55mm lens", Unit = "Unit", Price = 799.99m, Amount = 799.99m },
                new InvoiceItem { Id = 4, InvoiceId = 4, Service = "Hiking Boots", Description = "Waterproof hiking boots size US 9", Unit = "Unit", Price = 109.95m, Amount = 109.95m },
                new InvoiceItem { Id = 5, InvoiceId = 5, Service = "Skincare Set", Description = "Anti-aging skincare collection for normal skin", Unit = "Unit", Price = 75.99m, Amount = 75.99m },
                new InvoiceItem { Id = 6, InvoiceId = 6, Service = "Learning Robot", Description = "Interactive learning robot for kids", Unit = "Unit", Price = 69.99m, Amount = 69.99m },
                new InvoiceItem { Id = 7, InvoiceId = 7, Service = "Book Return", Description = "Credit for returned historical fiction book", Unit = "Unit", Price = -15.99m, Amount = -15.99m },
                new InvoiceItem { Id = 8, InvoiceId = 8, Service = "Smart Home Kit", Description = "Smart home starter kit with hub and bulbs", Unit = "Unit", Price = 149.99m, Amount = 149.99m },
                new InvoiceItem { Id = 9, InvoiceId = 1, Service = "Extended Warranty", Description = "2-year extended warranty for Smart TV", Unit = "Unit", Price = 49.99m, Amount = 49.99m },
                new InvoiceItem { Id = 10, InvoiceId = 3, Service = "Camera Tripod", Description = "Tripod accessory for DSLR camera", Unit = "Unit", Price = 29.99m, Amount = 29.99m }
            );

            // Seed InvoiceAttachments 
            modelBuilder.Entity<InvoiceAttachments>().HasData(
                new InvoiceAttachments { Id = 1, InvoiceId = 1, AttachmentName = "Invoice_INV-2025-001.pdf", AttachmentContent = "/files/invoices/INV-2025-001.pdf" },
                new InvoiceAttachments { Id = 2, InvoiceId = 2, AttachmentName = "Invoice_INV-2025-002.pdf", AttachmentContent = "/files/invoices/INV-2025-002.pdf" },
                new InvoiceAttachments { Id = 3, InvoiceId = 3, AttachmentName = "Invoice_INV-2025-003.pdf", AttachmentContent = "/files/invoices/INV-2025-003.pdf" },
                new InvoiceAttachments { Id = 4, InvoiceId = 4, AttachmentName = "Invoice_INV-2025-004.pdf", AttachmentContent = "/files/invoices/INV-2025-004.pdf" },
                new InvoiceAttachments { Id = 5, InvoiceId = 5, AttachmentName = "Invoice_INV-2025-005.pdf", AttachmentContent = "/files/invoices/INV-2025-005.pdf" },
                new InvoiceAttachments { Id = 6, InvoiceId = 6, AttachmentName = "Invoice_INV-2025-006.pdf", AttachmentContent = "/files/invoices/INV-2025-006.pdf" },
                new InvoiceAttachments { Id = 7, InvoiceId = 7, AttachmentName = "CreditNote_INV-2025-007.pdf", AttachmentContent = "/files/invoices/INV-2025-007.pdf" },
                new InvoiceAttachments { Id = 8, InvoiceId = 8, AttachmentName = "Invoice_INV-2025-008.pdf", AttachmentContent = "/files/invoices/INV-2025-008.pdf" }
            );

            // Seed TaxDetails 
            modelBuilder.Entity<TaxDetail>().HasData(
                new TaxDetail { Id = 1, InvoiceId = 1, TaxType = "VAT", Rate = 8.00m, Amount = 52.00m },
                new TaxDetail { Id = 2, InvoiceId = 2, TaxType = "GST", Rate = 8.00m, Amount = 10.40m, },
                new TaxDetail { Id = 3, InvoiceId = 3, TaxType = "Consumption Tax", Rate = 8.00m, Amount = 64.00m },
                new TaxDetail { Id = 4, InvoiceId = 4, TaxType = "GST", Rate = 8.00m, Amount = 8.80m },
                new TaxDetail { Id = 5, InvoiceId = 5, TaxType = "VAT", Rate = 8.00m, Amount = 6.08m },
                new TaxDetail { Id = 6, InvoiceId = 6, TaxType = "GST", Rate = 8.00m, Amount = 5.60m },
                new TaxDetail { Id = 7, InvoiceId = 7, TaxType = "VAT", Rate = 8.00m, Amount = -1.28m },
                new TaxDetail { Id = 8, InvoiceId = 8, TaxType = "VAT", Rate = 8.00m, Amount = 12.00m }
            );
        }
    }
}