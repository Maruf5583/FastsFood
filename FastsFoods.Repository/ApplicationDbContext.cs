
using FastsFood.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FastsFood.Repository
{   
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cupon> Coupons { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<OrderDetails> OrderDetails { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Fix cascade delete for Item - SubCategory
            builder.Entity<Item>()
                .HasOne(i => i.SubCategory)
                .WithMany(s => s.Items)
                .HasForeignKey(i => i.SubCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fix cascade delete for Item - Category
            builder.Entity<Item>()
                .HasOne(i => i.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fix cascade delete for SubCategory - Category
            builder.Entity<SubCategory>()
                .HasOne(s => s.Category)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Fix cascade delete for Cart - Item
            builder.Entity<Cart>()
                .HasOne(c => c.Item)
                .WithMany()
                .HasForeignKey(c => c.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fix cascade delete for OrderDetails - Item
            builder.Entity<OrderDetails>()
                .HasOne(od => od.Item)
                .WithMany()
                .HasForeignKey(od => od.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fix cascade delete for OrderDetails - OrderHeader
            builder.Entity<OrderDetails>()
                .HasOne(od => od.OrderHeader)
                .WithMany(oh => oh.OrderDetails)
                .HasForeignKey(od => od.OrderHeaderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}