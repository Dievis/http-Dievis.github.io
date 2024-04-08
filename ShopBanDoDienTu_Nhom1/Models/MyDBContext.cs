using System.Data.Entity;

namespace ShopBanDoDienTu_Nhom1.Models
{
    public class MyDBContext : DbContext
    {
        public MyDBContext() : base("MyConnectionString") { }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

    }
}