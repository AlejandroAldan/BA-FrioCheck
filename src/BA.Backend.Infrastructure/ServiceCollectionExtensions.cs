using BA.Backend.Application.Common.Interfaces;
using BA.Backend.Domain.Repositories;
using BA.Backend.Infrastructure.Data;
using BA.Backend.Infrastructure.Repositories;
using BA.Backend.Infrastructure.Services;
using BA.Backend.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BA.Backend.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings();
        configuration.GetSection("Jwt").Bind(jwtSettings);
        services.AddSingleton(jwtSettings);

        var databaseSettings = new DatabaseSettings();
        configuration.GetSection("ConnectionStrings").Bind(databaseSettings);
        services.AddSingleton(databaseSettings);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(databaseSettings.ConnectionString)
        );

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ISessionService, SessionService>();

        return services;
    }
}
