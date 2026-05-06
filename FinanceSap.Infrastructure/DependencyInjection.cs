using System.Text;
using FinanceSap.Domain.Interfaces;
using FinanceSap.Infrastructure.Identity;
using FinanceSap.Infrastructure.Persistence;
using FinanceSap.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Hosting;

namespace FinanceSap.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Priority: Environment Variable > appsettings.json
        // SQLite connection string (local file or in-memory for testing)
        var connectionString = Environment.GetEnvironmentVariable("SQLITE_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=financesap.db";

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(
                connectionString,
                sqlite => sqlite.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            )
        );

        // ASP.NET Core Identity — configuração com ApplicationUser customizado.
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            // Senha: requisitos mínimos para ambiente financeiro.
            options.Password.RequireDigit           = true;
            options.Password.RequireLowercase       = true;
            options.Password.RequireUppercase       = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength         = 8;

            // Lockout: proteção contra brute force.
            options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers      = true;

            // User: email único obrigatório.
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // JWT Authentication — tokens de curta duração (15 min).
        var jwtKey    = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurada.");
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "FinanceSap";
        var jwtAudience = configuration["Jwt:Audience"] ?? "FinanceSap";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer           = true,
                ValidateAudience         = true,
                ValidateLifetime         = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer              = jwtIssuer,
                ValidAudience            = jwtAudience,
                IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew                = TimeSpan.Zero // Remove tolerância padrão de 5 min.
            };
        });

        services.AddAuthorization();

        // Repositórios
        services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ILoanRepository, LoanRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // User Context — abstração para acesso ao usuário autenticado.
        services.AddScoped<IUserContext, UserContext>();

        return services;
    }
}
