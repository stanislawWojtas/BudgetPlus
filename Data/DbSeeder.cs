using BudgetPlus.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BudgetPlus.Data;

public static class DbSeeder
{
    public static async Task SeedAdminAsync(AppDbContext context)
    {
        if (!await context.Users.AnyAsync())
        {
            var adminUser = new User
            {
                Username = "admin",
                PasswordHash = HashPassword("admin123"),
                IsAdmin = true,
                ApiToken = Guid.NewGuid().ToString()
            };

            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();
        }
    }

    private static string HashPassword(string password)
    {
        using var md5 = MD5.Create();
        byte[] inputBytes = Encoding.ASCII.GetBytes(password);
        byte[] hashBytes = md5.ComputeHash(inputBytes);
        return Convert.ToHexString(hashBytes);
    }
}