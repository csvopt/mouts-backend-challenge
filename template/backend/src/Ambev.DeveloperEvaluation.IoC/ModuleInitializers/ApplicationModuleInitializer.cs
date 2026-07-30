using Ambev.DeveloperEvaluation.Application;
using Ambev.DeveloperEvaluation.Application.Common.Messaging;
using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.IoC.Messaging;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Transport.InMem;

namespace Ambev.DeveloperEvaluation.IoC.ModuleInitializers;

public class ApplicationModuleInitializer : IModuleInitializer
{
    public void Initialize(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        builder.Services.AddScoped<IEventPublisher, RebusEventPublisher>();
        builder.Services.AddRebus(configure => configure
            .Transport(transport => transport.UseInMemoryTransport(
                new InMemNetwork(),
                "developer-evaluation-events")));

        var validatorRegistrations = typeof(ApplicationLayer).Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .SelectMany(type => type.GetInterfaces()
                .Where(contract => contract.IsGenericType &&
                    contract.GetGenericTypeDefinition() == typeof(IValidator<>))
                .Select(contract => new { Contract = contract, Implementation = type }));

        foreach (var registration in validatorRegistrations)
            builder.Services.AddTransient(registration.Contract, registration.Implementation);
    }
}
