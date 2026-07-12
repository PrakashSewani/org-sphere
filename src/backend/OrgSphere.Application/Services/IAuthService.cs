using OrgSphere.Application.DTOs;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Application.Services;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<AuthResponseDto> RegisterAsync(string email, string password, string firstName, string lastName, CancellationToken ct = default);
    Task<AuthResponseDto> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task RevokeTokenAsync(string refreshToken, CancellationToken ct = default);
}
