using FinanceSap.Domain.Entities;
using FinanceSap.Domain.ValueObjects;
using FinanceSap.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceSap.Infrastructure.Persistence;

/// <summary>
/// DatabaseSeeder - Responsável por popular o banco de dados com dados iniciais
/// para desenvolvimento e testes. Executa apenas em ambiente Development.
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Método principal de seeding que é chamado na inicialização da aplicação.
    /// </summary>
    public static async Task SeedAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeder");
        var environment = services.GetRequiredService<IHostEnvironment>();

        // Executa apenas em ambiente de desenvolvimento
        if (!environment.IsDevelopment())
        {
            logger.LogInformation("Database seeding skipped - not in Development environment");
            return;
        }

        logger.LogInformation("Starting database seeding...");

        try
        {
            await SeedUsersAndCustomersAsync(services, logger);
            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error during database seeding");
            throw;
        }
    }

    /// <summary>
    /// Cria usuários e clientes padrão para desenvolvimento.
    /// </summary>
    private static async Task SeedUsersAndCustomersAsync(IServiceProvider services, ILogger logger)
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        // Aguarda a migração ser aplicada
        await dbContext.Database.EnsureCreatedAsync();

        // Cria roles se não existirem
        await CreateRolesAsync(roleManager);

        // Cria usuário Admin
        var adminUser = await CreateUserAsync(
            userManager,
            "admin@financesap.com",
            "Password123!",
            "Admin User",
            ["Admin"]);

        // Cria usuário Customer
        var customerUser = await CreateUserAsync(
            userManager,
            "customer@financesap.com",
            "Password123!",
            "Default Customer",
            ["Customer"]);

        // Cria cliente e conta para o usuário Customer se não existirem
        if (customerUser != null && !await dbContext.Customers.AnyAsync(c => c.Id == customerUser.CustomerId))
        {
            var customer = await CreateCustomerAsync(dbContext, customerUser.Id, "123.456.789-09", "Default Customer");
            var account = await CreateAccountAsync(dbContext, customer.Id, "1234567890", 10000.00m);

            customerUser.CustomerId = customer.Id;
            await userManager.UpdateAsync(customerUser);

            logger.LogInformation("Created customer with initial balance: $10,000.00");
        }
    }

    /// <summary>
    /// Cria roles no sistema se não existirem.
    /// </summary>
    private static async Task CreateRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        var roles = new[] { "Admin", "Customer" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole<Guid>(roleName);
                await roleManager.CreateAsync(role);
            }
        }
    }

    /// <summary>
    /// Cria um usuário no sistema com roles especificados.
    /// </summary>
    private static async Task<ApplicationUser?> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string fullName,
        string[] roles)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return existingUser;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new Exception($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Adiciona roles ao usuário
        foreach (var role in roles)
        {
            await userManager.AddToRoleAsync(user, role);
        }

        return user;
    }

    /// <summary>
    /// Cria um cliente no sistema.
    /// </summary>
    private static async Task<Customer> CreateCustomerAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        string cpf,
        string fullName)
    {
        var customerResult = Customer.Create(cpf, fullName);
        if (!customerResult.IsSuccess)
        {
            throw new Exception($"Failed to create customer: {customerResult.Error}");
        }

        var customer = customerResult.Value;
        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync();

        return customer;
    }

    /// <summary>
    /// Cria uma conta bancária para um cliente com saldo inicial.
    /// </summary>
    private static async Task<Account> CreateAccountAsync(
        ApplicationDbContext dbContext,
        Guid customerId,
        string accountNumber,
        decimal initialBalance)
    {
        var accountResult = Account.Create(accountNumber, customerId);
        if (!accountResult.IsSuccess)
        {
            throw new Exception($"Failed to create account: {accountResult.Error}");
        }

        var account = accountResult.Value;
        account.Credit(initialBalance); // Adiciona saldo inicial

        dbContext.Accounts.Add(account);
        await dbContext.SaveChangesAsync();

        return account;
    }

    // Helper para logging (removido - usando injeção de dependência)
}