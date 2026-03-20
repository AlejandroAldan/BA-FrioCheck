// using BA.Backend.Application.Auth.Commands;  // TODO: Descomentar cuando haya comandos en Auth
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BA.Backend.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}

