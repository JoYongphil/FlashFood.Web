using FlashFood.Web.Data;
using FlashFood.Web.Models.Entities;
using FlashFood.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FlashFood.Web.Services;

public interface IVoucherService
{
    Task EnsureVoucherForUserAsync(string userId);
    Task<Voucher?> ValidateVoucherAsync(string code, string userId, VoucherType expectedType);
    Task<List<Voucher>> GetActiveVouchersAsync(string userId);
}

public class VoucherService(ApplicationDbContext dbContext) : IVoucherService
{
    public async Task EnsureVoucherForUserAsync(string userId)
    {
        var todayUtc = DateTime.UtcNow.Date;

        var hasVoucherToday = await dbContext.Vouchers.AnyAsync(x =>
            x.UserId == userId &&
            x.CreatedAt >= todayUtc &&
            x.CreatedAt < todayUtc.AddDays(1));

        if (hasVoucherToday)
        {
            return;
        }

        var random = Random.Shared.Next(1, 101);
        Voucher voucher;

        if (random <= 25)
        {
            voucher = new Voucher
            {
                UserId = userId,
                Code = $"FREE{Guid.NewGuid():N}"[..10].ToUpperInvariant(),
                Type = VoucherType.FreeShip,
                Value = 100,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
        }
        else
        {
            var percent = Random.Shared.Next(5, 21);
            voucher = new Voucher
            {
                UserId = userId,
                Code = $"SALE{Guid.NewGuid():N}"[..10].ToUpperInvariant(),
                Type = VoucherType.Percentage,
                Value = percent,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
        }

        dbContext.Vouchers.Add(voucher);
        await dbContext.SaveChangesAsync();
    }

    public async Task<Voucher?> ValidateVoucherAsync(string code, string userId, VoucherType expectedType)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return await dbContext.Vouchers.FirstOrDefaultAsync(x =>
            x.Code == code.Trim().ToUpper() &&
            x.UserId == userId &&
            x.Type == expectedType &&
            !x.IsUsed &&
            x.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<List<Voucher>> GetActiveVouchersAsync(string userId)
    {
        return await dbContext.Vouchers
            .Where(x => x.UserId == userId && !x.IsUsed && x.ExpiresAt > DateTime.UtcNow)
            .OrderBy(x => x.ExpiresAt)
            .ToListAsync();
    }
}
