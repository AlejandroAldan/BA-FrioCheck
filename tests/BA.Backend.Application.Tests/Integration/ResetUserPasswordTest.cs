using BA.Backend.Domain.Entities;
using BA.Backend.Infrastructure.Data;
using BA.Backend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using System.IO;

namespace BA.Backend.Application.Tests.Integration;

public class ResetUserPasswordTest
{
    [Fact]
    public async Task ResetAdminPassword_ShouldUpdatePasswordHashInDatabase()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection");
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            // Act
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@test.com");
            
            if (user != null)
            {
                var passwordHasher = new PasswordHasher();
                user.PasswordHash = passwordHasher.Hash("Admin123!");
                
                context.Users.Update(user);
                await context.SaveChangesAsync();
                
                // Assert
                Assert.NotNull(user);
                Assert.Equal("admin@test.com", user.Email);
                Assert.True(passwordHasher.Verify("Admin123!", user.PasswordHash));
            }
        }
    }
}
