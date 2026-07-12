using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrgSphere.Application.DTOs;
using OrgSphere.Application.Services;
using OrgSphere.Domain.Configuration;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Auth;

namespace OrgSphere.Infrastructure.Services;

public class AuthService(
    IUnitOfWork unitOfWork,
    IRefreshTokenStore refreshTokenStore,
    IOptions<JwtSettings> jwtSettings,
    ILogger<AuthService> logger) : IAuthService
{
    public async Task<AuthResponseDto> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var settings = jwtSettings.Value;

        var user = await FindUserByEmailAsync(email, ct)
            ?? throw new UnauthorizedAccessException("Invalid email or password");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Account is deactivated");

        user.LastLoginAt = DateTime.UtcNow;
        await unitOfWork.Users.UpdateAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var loginEmail = email;
        logger.LogInformation("User {Email} logged in successfully", loginEmail);

        return await GenerateTokensAsync(user, settings, ct);
    }

    public async Task<AuthResponseDto> RegisterAsync(string email, string password, string firstName, string lastName, CancellationToken ct = default)
    {
        var settings = jwtSettings.Value;

        var existing = await unitOfWork.Users.GetByEmailGlobalAsync(email, ct);
        if (existing is not null)
            throw new InvalidOperationException("Email already registered");

        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            FirstName = firstName,
            LastName = lastName,
            Role = UserRole.Employee,
            IsActive = true
        };

        var created = await unitOfWork.Users.CreateAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return await GenerateTokensAsync(created, settings, ct);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var settings = jwtSettings.Value;

        var tokenData = await refreshTokenStore.GetAsync(refreshToken, ct)
            ?? throw new UnauthorizedAccessException("Invalid or expired refresh token");

        await refreshTokenStore.RevokeAsync(refreshToken, ct);

        var user = await unitOfWork.Users.GetByIdAsync(tokenData.UserId, tokenData.TenantId, ct)
            ?? throw new UnauthorizedAccessException("User not found");

        return await GenerateTokensAsync(user, settings, ct);
    }

    public async Task RevokeTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        await refreshTokenStore.RevokeAsync(refreshToken, ct);
    }

    private async Task<AuthResponseDto> GenerateTokensAsync(User user, JwtSettings settings, CancellationToken ct)
    {
        var accessToken = GenerateAccessToken(user, settings);
        var refreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(settings.AccessTokenExpirationMinutes);

        await refreshTokenStore.SaveAsync(refreshToken, user.Id, user.TenantId, DateTime.UtcNow.AddDays(settings.RefreshTokenExpirationDays), ct);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id.Value,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            }
        };
    }

    private static string GenerateAccessToken(User user, JwtSettings settings)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("tenantId", user.TenantId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(settings.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private async Task<User?> FindUserByEmailAsync(string email, CancellationToken ct)
    {
        return await unitOfWork.Users.GetByEmailGlobalAsync(email, ct);
    }
}
