using DispatchR.Abstractions.Send;
using DispatchR.Extensions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using OrgSphere.Application.Behaviors;

namespace OrgSphere.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddDispatchR(typeof(ApplicationServiceCollectionExtensions).Assembly);

        services.AddValidatorsFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }
}
