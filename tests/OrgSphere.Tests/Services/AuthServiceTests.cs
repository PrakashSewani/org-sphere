using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using OrgSphere.Domain.Configuration;
using OrgSphere.Domain.Enums;
using OrgSphere.Domain.ValueObjects;
using OrgSphere.Infrastructure.Auth;
using OrgSphere.Infrastructure.Services;
using OrgSphere.Tests.Fixtures;

namespace OrgSphere.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly MockUnitOfWorkFixture _fixture = new();
    private readonly Mock<IRefreshTokenStore> _refreshTokenStore = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var jwtSettings = Options.Create(new JwtSettings
        {
            Secret = "TestSecret-Key-256Bits-LongEnoughForHmacSha256!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 30
        });

        _refreshTokenStore.Setup(r => r.SaveAsync(It.IsAny<string>(), It.IsAny<UserId>(), It.IsAny<TenantId>(),
            It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new AuthService(
            _fixture.UnitOfWork.Object,
            _refreshTokenStore.Object,
            jwtSettings,
            new Microsoft.Extensions.Logging.Abstractions.NullLogger<AuthService>());
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTokens_WhenValidCredentials()
    {
        var user = _fixture.CreateTestUser();
        _fixture.UserRepository.Setup(r => r.GetByEmailGlobalAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.LoginAsync(user.Email, "Password123!");

        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Email.Should().Be(user.Email);
        _refreshTokenStore.Verify(r => r.SaveAsync(It.IsAny<string>(), user.Id, user.TenantId,
            It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenEmailNotFound()
    {
        _fixture.UserRepository.Setup(r => r.GetByEmailGlobalAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrgSphere.Domain.Entities.User?)null);

        var act = () => _sut.LoginAsync("notfound@test.com", "Password123!");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordIncorrect()
    {
        var user = _fixture.CreateTestUser();
        _fixture.UserRepository.Setup(r => r.GetByEmailGlobalAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var act = () => _sut.LoginAsync(user.Email, "WrongPassword!");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserInactive()
    {
        var user = _fixture.CreateTestUser();
        user.IsActive = false;
        _fixture.UserRepository.Setup(r => r.GetByEmailGlobalAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var act = () => _sut.LoginAsync(user.Email, "Password123!");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Account is deactivated*");
    }

    [Fact]
    public async Task RegisterAsync_ShouldCreateUserAndReturnTokens()
    {
        _fixture.UserRepository.Setup(r => r.GetByEmailGlobalAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrgSphere.Domain.Entities.User?)null);
        _fixture.UserRepository.Setup(r => r.CreateAsync(It.IsAny<OrgSphere.Domain.Entities.User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrgSphere.Domain.Entities.User u, CancellationToken _) =>
            {
                u.Id = UserId.New();
                u.TenantId = TenantId.New();
                return u;
            });

        var result = await _sut.RegisterAsync("new@test.com", "Password123!", "New", "User");

        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.User.Email.Should().Be("new@test.com");
        result.User.FirstName.Should().Be("New");
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmailAlreadyExists()
    {
        var existingUser = _fixture.CreateTestUser();
        _fixture.UserRepository.Setup(r => r.GetByEmailGlobalAsync(existingUser.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var act = () => _sut.RegisterAsync(existingUser.Email, "Password123!", "Test", "User");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Email already registered*");
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldReturnNewTokens()
    {
        var user = _fixture.CreateTestUser();
        _refreshTokenStore.Setup(r => r.GetAsync("valid-refresh-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((user.Id, user.TenantId));
        _fixture.UserRepository.Setup(r => r.GetByIdAsync(user.Id, user.TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.RefreshTokenAsync("valid-refresh-token");

        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        _refreshTokenStore.Verify(r => r.RevokeAsync("valid-refresh-token", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_ShouldThrow_WhenTokenInvalid()
    {
        _refreshTokenStore.Setup(r => r.GetAsync("invalid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(((UserId, TenantId)?)null);

        var act = () => _sut.RefreshTokenAsync("invalid-token");

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Invalid or expired refresh token*");
    }

    [Fact]
    public async Task RevokeTokenAsync_ShouldRevokeToken()
    {
        await _sut.RevokeTokenAsync("some-token");

        _refreshTokenStore.Verify(r => r.RevokeAsync("some-token", It.IsAny<CancellationToken>()), Times.Once);
    }
}
