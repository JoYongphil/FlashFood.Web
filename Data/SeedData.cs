using FlashFood.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FlashFood.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await db.Database.EnsureCreatedAsync();
        await EnsureCategoryIconSvgColumnAsync(db);

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        const string adminEmail = "admin@flashfood.vn";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrator",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin@123");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (await db.Categories.AnyAsync())
        {
            return;
        }

        var categories = new List<Category>
        {
            new() { Name = "Burgers", Description = "Signature burgers" },
            new() { Name = "Ga ran", Description = "Crispy fried chicken" },
            new() { Name = "Mon phu", Description = "Side dishes" },
            new() { Name = "Do uong", Description = "Refreshing drinks" }
        };

        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();

        var products = new List<Product>
        {
            new() { Name = "Flash Beef Burger", BasePrice = 89000, Description = "Beef patty, cheese and special sauce.", ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=1200", CategoryId = categories[0].Id },
            new() { Name = "Double Cheese Burger", BasePrice = 99000, Description = "Double beef, double cheese.", ImageUrl = "https://images.unsplash.com/photo-1550547660-d9450f859349?w=1200", CategoryId = categories[0].Id },
            new() { Name = "Chicken Burger", BasePrice = 79000, Description = "Crispy chicken burger.", ImageUrl = "https://images.unsplash.com/photo-1606755962773-d324e0a13086?w=1200", CategoryId = categories[0].Id },
            new() { Name = "BBQ Bacon Burger", BasePrice = 109000, Description = "Beef burger with bacon and BBQ sauce.", ImageUrl = "https://images.unsplash.com/photo-1571091718767-18b5b1457add?w=1200", CategoryId = categories[0].Id },
            new() { Name = "Mushroom Burger", BasePrice = 95000, Description = "Creamy mushroom sauce burger.", ImageUrl = "https://images.unsplash.com/photo-1561758033-d89a9ad46330?w=1200", CategoryId = categories[0].Id },
            new() { Name = "Fried Chicken Original", BasePrice = 69000, Description = "2 pieces crispy fried chicken.", ImageUrl = "https://images.unsplash.com/photo-1626645738196-c2a7c87a8f58?w=1200", CategoryId = categories[1].Id },
            new() { Name = "Spicy Wings", BasePrice = 75000, Description = "Wings with spicy sauce.", ImageUrl = "https://images.unsplash.com/photo-1527477396000-e27163b481c2?w=1200", CategoryId = categories[1].Id },
            new() { Name = "Honey Chicken", BasePrice = 79000, Description = "Crispy chicken with honey glaze.", ImageUrl = "https://images.unsplash.com/photo-1518492104633-130d0cc84637?w=1200", CategoryId = categories[1].Id },
            new() { Name = "Chicken Combo", BasePrice = 129000, Description = "4 pieces chicken with fries and drink.", ImageUrl = "https://images.unsplash.com/photo-1513639776629-7b61b0ac49cb?w=1200", CategoryId = categories[1].Id },
            new() { Name = "Chicken Tenders", BasePrice = 72000, Description = "Tender strips with pepper sauce.", ImageUrl = "https://images.unsplash.com/photo-1585238342024-78d387f4a707?w=1200", CategoryId = categories[1].Id },
            new() { Name = "French Fries", BasePrice = 35000, Description = "Golden crispy fries.", ImageUrl = "https://images.unsplash.com/photo-1630384060421-cb20d0e0649d?w=1200", CategoryId = categories[2].Id },
            new() { Name = "Cheese Fries", BasePrice = 42000, Description = "Fries with cheese seasoning.", ImageUrl = "https://images.unsplash.com/photo-1639744211487-a35b76dffe4d?w=1200", CategoryId = categories[2].Id },
            new() { Name = "Nuggets", BasePrice = 45000, Description = "Crispy chicken nuggets.", ImageUrl = "https://images.unsplash.com/photo-1512152272829-e3139592d56f?w=1200", CategoryId = categories[2].Id },
            new() { Name = "Salad", BasePrice = 39000, Description = "Fresh salad.", ImageUrl = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=1200", CategoryId = categories[2].Id },
            new() { Name = "Onion Rings", BasePrice = 43000, Description = "Crispy onion rings.", ImageUrl = "https://images.unsplash.com/photo-1601050690597-df0568f70950?w=1200", CategoryId = categories[2].Id },
            new() { Name = "Coca Cola", BasePrice = 18000, Description = "Cold coke drink.", ImageUrl = "https://images.unsplash.com/photo-1581636625402-29b2a704ef13?w=1200", CategoryId = categories[3].Id },
            new() { Name = "Pepsi", BasePrice = 18000, Description = "Cold pepsi drink.", ImageUrl = "https://images.unsplash.com/photo-1629203432180-71e9b2f87782?w=1200", CategoryId = categories[3].Id },
            new() { Name = "Peach Tea", BasePrice = 32000, Description = "Iced peach tea.", ImageUrl = "https://images.unsplash.com/photo-1499636136210-6f4ee915583e?w=1200", CategoryId = categories[3].Id },
            new() { Name = "Lemon Tea", BasePrice = 28000, Description = "Iced lemon tea.", ImageUrl = "https://images.unsplash.com/photo-1521302200778-33500795e128?w=1200", CategoryId = categories[3].Id },
            new() { Name = "Orange Juice", BasePrice = 30000, Description = "Fresh orange juice.", ImageUrl = "https://images.unsplash.com/photo-1600271886742-f049cd451bba?w=1200", CategoryId = categories[3].Id }
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync();

        var variants = new List<ProductVariant>();
        foreach (var product in products)
        {
            variants.Add(new ProductVariant { ProductId = product.Id, Name = "Size M", AdditionalPrice = 0 });
            variants.Add(new ProductVariant { ProductId = product.Id, Name = "Size L", AdditionalPrice = 10000 });
        }

        db.ProductVariants.AddRange(variants);
        await db.SaveChangesAsync();
    }

    private static async Task EnsureCategoryIconSvgColumnAsync(ApplicationDbContext db)
    {
        const string sql = @"
IF COL_LENGTH('dbo.Categories', 'IconSvg') IS NULL
BEGIN
    ALTER TABLE [dbo].[Categories] ADD [IconSvg] nvarchar(max) NULL;
END";

        await db.Database.ExecuteSqlRawAsync(sql);
    }
}

