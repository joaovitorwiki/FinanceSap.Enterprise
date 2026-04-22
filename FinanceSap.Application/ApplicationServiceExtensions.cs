using FinanceSap.Application.Behaviors;
using FinanceSap.Application.UseCases.CreateCustomer;
using FinanceSap.Application.UseCases.CreateLoanApplication;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceSap.Application;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Handlers legados (pré-MediatR) — manter compatibilidade.
        services.AddScoped<CreateCustomerHandler>();
        services.AddScoped<CreateLoanApplicationUseCase>();

        // MediatR — registra todos os IRequestHandler e INotificationHandler da assembly.
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<CreateCustomerHandler>();
            // Pipeline Behavior — intercepta todos os requests e executa validação.
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // FluentValidation — escaneia toda a assembly e registra todos os AbstractValidator<T>.
        services.AddValidatorsFromAssemblyContaining<CreateCustomerHandler>();

        return services;
    }
}
