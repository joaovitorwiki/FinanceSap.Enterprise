using FinanceSap.Domain.Entities;
using FinanceSap.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceSap.Infrastructure.Repositories;

/// <summary>
/// Repository for managing RefreshToken entities.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Adds a new refresh token to the database.
    /// </summary>
    /// <param name="refreshToken">The refresh token to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a refresh token by its token value.
    /// </summary>
    /// <param name="token">The refresh token value.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The refresh token if found, otherwise null.</returns>
    Task<RefreshToken?> FindByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds all active refresh tokens for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of active refresh tokens.</returns>
    Task<List<RefreshToken>> FindActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all active refresh tokens for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="revokedByIp">The IP address that revoked the tokens.</param>
    /// <param name="reason">The reason for revocation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RevokeAllForUserAsync(Guid userId, string revokedByIp, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all expired refresh tokens from the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveExpiredAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementation of IRefreshTokenRepository.
/// </summary>
public sealed class RefreshTokenRepository(ApplicationDbContext context) : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context = context;

    /// <inheritdoc />
    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RefreshToken?> FindByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<RefreshToken>> FindActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task RevokeAllForUserAsync(Guid userId, string revokedByIp, string reason, CancellationToken cancellationToken = default)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.Revoke(revokedByIp, reason);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveExpiredAsync(CancellationToken cancellationToken = default)
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => (rt.RevokedAt != null || rt.ExpiresAt <= DateTime.UtcNow) && rt.ExpiresAt < DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync(cancellationToken);
    }
}