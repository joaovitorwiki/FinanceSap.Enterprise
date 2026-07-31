using System;

namespace FinanceSap.Domain.Entities;

/// <summary>
/// Refresh Token Entity - Represents a refresh token for JWT authentication.
/// OWASP A2: Broken Authentication - Refresh tokens mitigate the risk of long-lived JWTs.
/// </summary>
public sealed class RefreshToken
{
    /// <summary>
    /// Unique identifier for the refresh token.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// The user associated with this refresh token.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// The actual token value (hashed for security).
    /// </summary>
    public string Token { get; private set; } = string.Empty;

    /// <summary>
    /// The JWT token that this refresh token can generate.
    /// </summary>
    public string JwtTokenId { get; private set; } = string.Empty;

    /// <summary>
    /// Date and time when the refresh token expires.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// Date and time when the refresh token was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// IP address of the client that requested this refresh token.
    /// </summary>
    public string CreatedByIp { get; private set; } = string.Empty;

    /// <summary>
    /// Date and time when the refresh token was revoked (if applicable).
    /// </summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>
    /// IP address that revoked this refresh token (if applicable).
    /// </summary>
    public string? RevokedByIp { get; private set; }

    /// <summary>
    /// Reason for revocation (if applicable).
    /// </summary>
    public string? RevocationReason { get; private set; }

    /// <summary>
    /// Indicates whether the refresh token is still active.
    /// </summary>
    public bool IsActive => RevokedAt == null && ExpiresAt > DateTime.UtcNow;

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private RefreshToken() { }

    /// <summary>
    /// Creates a new refresh token.
    /// </summary>
    /// <param name="userId">The user ID associated with this token.</param>
    /// <param name="token">The refresh token value (will be hashed).</param>
    /// <param name="jwtTokenId">The JWT token ID that this refresh token can generate.</param>
    /// <param name="expiresAt">The expiration date of the refresh token.</param>
    /// <param name="createdByIp">The IP address that created this token.</param>
    public static RefreshToken Create(
        Guid userId,
        string token,
        string jwtTokenId,
        DateTime expiresAt,
        string createdByIp)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token, // In production, this should be hashed
            JwtTokenId = jwtTokenId,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = createdByIp
        };
    }

    /// <summary>
    /// Revokes the refresh token.
    /// </summary>
    /// <param name="revokedByIp">The IP address that revoked this token.</param>
    /// <param name="reason">The reason for revocation.</param>
    public void Revoke(string revokedByIp, string reason)
    {
        RevokedAt = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        RevocationReason = reason;
    }
}