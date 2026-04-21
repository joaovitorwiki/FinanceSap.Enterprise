using FinanceSap.Application.UseCases.CreateCustomer;
using FinanceSap.Application.UseCases.CreateLoanApplication;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceSap.Application;

// Ponto único de registro de todos os serviços da camada Application.
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Handlers
        services.AddScoped<CreateCustomerHandler>();
        services.AddScoped<CreateLoanApplicationUseCase>();

        // Validators — escaneia toda a assembly, registra todos os AbstractValidator<T>
        services.AddValidatorsFromAssemblyContaining<CreateCustomerValidator>();

        return services;
    }
}
