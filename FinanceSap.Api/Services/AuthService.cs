using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FinanceSap.Domain.Entities;
using FinanceSap.Domain.Interfaces;
using FinanceSap.Infrastructure.Identity;
using FinanceSap.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FinanceSap.Api.Services;

/// <summary>
/// Service for handling authentication operations including JWT generation and refresh token management.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user and generates JWT and refresh tokens.
    /// </summary>
    /// <param name="email">User email.</param>
    /// <param name="password">User password.</param>
    /// <param name="ipAddress">Client IP address.</param>
    /// <returns>Authentication result with tokens.</returns>
    Task<AuthResult> AuthenticateAsync(string email, string password, string ipAddress);

    /// <summary>
    /// Refreshes an expired JWT using a valid refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="ipAddress">Client IP address.</param>
    /// <returns>Authentication result with new tokens.</returns>
    Task<AuthResult> RefreshTokenAsync(string refreshToken, string ipAddress);

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to revoke.</param>
    /// <param name="ipAddress">Client IP address.</param>
    /// <param name="reason">Reason for revocation.</param>
    Task RevokeTokenAsync(string refreshToken, string ipAddress, string reason);
}

/// <summary>
/// Implementation of IAuthService.
/// </summary>
public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    IRefreshTokenRepository refreshTokenRepository,
    ICustomerRepository customerRepository) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IConfiguration _configuration = configuration;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly ICustomerRepository _customerRepository = customerRepository;

    /// <inheritdoc />
    public async Task<AuthResult> AuthenticateAsync(string email, string password, string ipAddress)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return new AuthResult(false, "Credenciais inválidas", null, null, null);
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!isPasswordValid)
        {
            return new AuthResult(false, "Credenciais inválidas", null, null, null);
        }

        // Revoke any existing active tokens for this user
        await _refreshTokenRepository.RevokeAllForUserAsync(
            user.Id,
            ipAddress,
            "New login detected"
        );

        // Generate new tokens
        var jwtToken = GenerateJwtToken(user);
        var refreshToken = await GenerateRefreshToken(user, ipAddress);

        // Get user data including customer information
        var userData = await GetUserDataAsync(user);

        return new AuthResult(true, null, jwtToken, refreshToken, userData);
    }

    /// <inheritdoc />
     public async Task<AuthResult> RefreshTokenAsync(string token, string ipAddress)
     {
         var refreshToken = await _refreshTokenRepository.FindByTokenAsync(token);
        if (refreshToken is null || refreshToken.RevokedAt != null || refreshToken.ExpiresAt <= DateTime.UtcNow)
        {
            return new AuthResult(false, "Refresh token inválido ou expirado", null, null, null);
        }

        var user = await _userManager.FindByIdAsync(refreshToken.UserId.ToString());
        if (user is null)
        {
            return new AuthResult(false, "Usuário não encontrado", null, null, null);
        }

        // Revoke the current refresh token
        refreshToken.Revoke(ipAddress, "Token refreshed");
        await _refreshTokenRepository.AddAsync(refreshToken);

        // Generate new tokens
        var jwtToken = GenerateJwtToken(user);
        var newRefreshToken = await GenerateRefreshToken(user, ipAddress);

        // Get user data including customer information
        var userData = await GetUserDataAsync(user);

        return new AuthResult(true, null, jwtToken, newRefreshToken, userData);
    }

    /// <inheritdoc />
     public async Task RevokeTokenAsync(string token, string ipAddress, string reason)
     {
         var refreshToken = await _refreshTokenRepository.FindByTokenAsync(token);
         if (refreshToken is null || refreshToken.RevokedAt != null || refreshToken.ExpiresAt <= DateTime.UtcNow)
         {
             return;
         }

        refreshToken.Revoke(ipAddress, reason);
        await _refreshTokenRepository.AddAsync(refreshToken);
    }

    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user to generate the token for.</param>
    /// <returns>The generated JWT token.</returns>
    private string GenerateJwtToken(ApplicationUser user)
    {
        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurada.");
        var issuer = _configuration["Jwt:Issuer"] ?? "FinanceSap";
        var audience = _configuration["Jwt:Audience"] ?? "FinanceSap";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("customerId", user.CustomerId?.ToString() ?? string.Empty)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15), // Token de curta duração
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a refresh token for the specified user.
    /// </summary>
    /// <param name="user">The user to generate the refresh token for.</param>
    /// <param name="ipAddress">Client IP address.</param>
    /// <returns>The generated refresh token.</returns>
    private async Task<string> GenerateRefreshToken(ApplicationUser user, string ipAddress)
    {
        // Generate a random token
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var token = Convert.ToBase64String(randomNumber);

        // Get the JWT token ID from the current JWT token
        var jwtTokenId = Guid.NewGuid().ToString();

        // Create refresh token entity
        var refreshToken = RefreshToken.Create(
            user.Id,
            token,
            jwtTokenId,
            DateTime.UtcNow.AddDays(7), // Refresh token expires in 7 days
            ipAddress
        );

        await _refreshTokenRepository.AddAsync(refreshToken);

        return token;
    }

    /// <summary>
    /// Gets user data including customer information for authentication responses.
    /// </summary>
    /// <param name="user">The application user.</param>
    /// <returns>User data with all required fields.</returns>
    private async Task<UserData> GetUserDataAsync(ApplicationUser user)
    {
        // Get user roles
        var roles = (await _userManager.GetRolesAsync(user)).ToArray();

        // If user has a customer, get customer data
        Customer? customer = null;
        if (user.CustomerId.HasValue)
        {
            customer = await _customerRepository.GetByIdAsync(user.CustomerId.Value);
        }

        return new UserData(
            Id: user.Id,
            Email: user.Email!,
            Name: customer?.FullName ?? user.UserName ?? "Unknown",
            Document: customer?.Document ?? "00000000000",
            Roles: roles
        );
    }
}

/// <summary>
/// Result of authentication operations.
/// </summary>
public sealed record AuthResult(
    bool Success,
    string? ErrorMessage,
    string? JwtToken,
    string? RefreshToken,
    UserData? User);

/// <summary>
/// User data returned in authentication responses.
/// </summary>
public sealed record UserData(
    Guid Id,
    string Email,
    string Name,
    string Document,
    string[] Roles);
