using FlashFood.Web.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FlashFood.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>()
            .Property(x => x.Name)
            .HasMaxLength(120);

        builder.Entity<Product>()
            .Property(x => x.Name)
            .HasMaxLength(200);

        builder.Entity<ProductVariant>()
            .Property(x => x.Name)
            .HasMaxLength(120);

        builder.Entity<Voucher>()
            .HasIndex(x => x.Code)
            .IsUnique();

        builder.Entity<Order>()
            .HasIndex(x => x.OrderCode)
            .IsUnique();

        builder.Entity<Review>()
            .Property(x => x.Rating)
            .HasDefaultValue(5);
    }
}


