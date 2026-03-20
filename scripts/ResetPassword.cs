using BA.Backend.Infrastructure.Data;
using BA.Backend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

// Cargar configuración
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Crear DbContext
var connectionString = config.GetConnectionString("DefaultConnection");
var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseSqlServer(connectionString)
    .Options;

using (var context = new ApplicationDbContext(options))
{
    try
    {
        // Buscar el usuario admin
        var user = context.Users.FirstOrDefault(u => u.Email == "admin@test.com");
        
        if (user != null)
        {
            // Crear el hash correcto usando BCrypt
            var hasher = new PasswordHasher();
            user.PasswordHash = hasher.Hash("Admin123!");
            
            context.Users.Update(user);
            context.SaveChanges();
            
            Console.WriteLine("✅ Usuario actualizado exitosamente");
            Console.WriteLine($"Email: {user.Email}");
            Console.WriteLine($"Hash: {user.PasswordHash}");
        }
        else
        {
            Console.WriteLine(" Usuario no encontrado");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($" Error: {ex.Message}");
    }
}
