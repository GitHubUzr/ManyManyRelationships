using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManyManyRelationships
{
    class Store
    {
        public int StoreID { get; set; }
        public string StoreName { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public List<StoreStock> ProductsStocked { get; set; }
    }

    class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public List<StoreStock> ProductDistribution { get; set; }
        public List<OrderItem> ProductOrders { get; set; }
    }

    class StoreStock
    {
        public int StoreStockID { get; set; }
        public Store StockLocation { get; set; }
        public Product StockProduct { get; set; }
        public int StockQty { get; set; }
    }

    class Order
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItem> ItemsOrdered { get; set; }
    }

    class OrderItem
    {
        public int OrderItemID { get; set; }
        public Product ProductOrdered { get; set; }
        public Order MainOrder { get; set; }
        public int ItemQty { get; set; }
    }

    class StoreOrdersContext : DbContext
    {
        public DbSet<Store> Stores { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<StoreStock> StoreStocks { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ManyManyDemo;Trusted_Connection=True;MultipleActiveResultSets=true";
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (StoreOrdersContext context = new StoreOrdersContext())
            {
                context.Database.EnsureCreated();
                Store store = new Store 
                { 
                    StoreName = "University Plaza", 
                    City="Tampa", 
                    State="FL"
                };
                Product productA = new Product { ProductName = "Colombian Coffee Grounds" };
                Product productB = new Product { ProductName = "Stainless Steel Travel Mug" };
                StoreStock storeStockA = new StoreStock
                {
                    StockLocation = store,
                    StockProduct = productA,
                    StockQty = 17
                };
                StoreStock storeStockB = new StoreStock
                {
                    StockLocation = store,
                    StockProduct = productB,
                    StockQty = 20
                };
                Order orderA = new Order { OrderDate = DateTime.Now };
                Order orderB = new Order { OrderDate = DateTime.Now.AddDays(-1) };
                Order orderC = new Order { OrderDate = DateTime.Now.AddDays(-2) };
                OrderItem orderItemA = new OrderItem
                {
                    ProductOrdered = productA,
                    MainOrder = orderA,
                    ItemQty = 2
                };
                OrderItem orderItemB = new OrderItem
                {
                    ProductOrdered = productB,
                    MainOrder = orderB,
                    ItemQty = 1
                };
                OrderItem orderItemC = new OrderItem
                {
                    ProductOrdered = productA,
                    MainOrder = orderC,
                    ItemQty = 1
                };

                context.Stores.Add(store);
                context.Products.Add(productA);
                context.Products.Add(productB);
                context.StoreStocks.Add(storeStockA);
                context.StoreStocks.Add(storeStockB);
                context.Orders.Add(orderA);
                context.Orders.Add(orderB);
                context.Orders.Add(orderC);
                context.OrderItems.Add(orderItemA);
                context.OrderItems.Add(orderItemB);
                context.OrderItems.Add(orderItemC);

                context.SaveChanges();

                //LINQ Queries
                //List all orders where a product is sold.
                //Unsure of query interpretation, listing several possible solutions below
                //A - List all orders (all orders have products sold)
                var Query1A = context.Orders
                                        .Include(o => o.ItemsOrdered)
                                        .ToList();
                //B - List associated order information for order items where products sold >=1
                var Query1B = context.OrderItems
                                        .Include(o => o.MainOrder)
                                        .Where(o => o.ItemQty >= 1)
                                        .ToList();
                Product TestQuery = context.Products
                        .Where(p => p.ProductName == "Colombian Coffee Grounds")
                        .FirstOrDefault();
                //C - List associated order information for order items where a specific product, coffee, is sold.
                var Query1C = context.OrderItems
                        .Include(o => o.MainOrder)
                        .Include(o => o.ProductOrdered)
                        .Where(o => o.ProductOrdered.ProductName == "Colombian Coffee Grounds" )
                        .ToList();

                //For a given product, find the order where it is sold the maximum.
                //list associated order information for order item where coffee is sold the maximum qty
                OrderItem Query2 = context.OrderItems
                                      .Include(o => o.MainOrder)
                                      .Include(o => o.ProductOrdered)
                                      .Where(o => o.ProductOrdered.ProductName == "Colombian Coffee Grounds")
                                      .OrderByDescending(o => o.ItemQty)
                                      .FirstOrDefault();
            }
        }
    }
}
