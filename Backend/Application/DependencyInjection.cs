using Application;
using Application.Behaviors;
using Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ExceptionHandlingBehavior<,>));

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(PerformanceBehavior<,>));

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(LoggingBehavior<,>));
        return services;
    }
}






