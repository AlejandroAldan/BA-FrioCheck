using BA.Backend.Application.Auth.Commands;
using BA.Backend.Application.Auth.Handlers;
using BA.Backend.Application.Auth.DTOs;
using BA.Backend.Domain.Entities;
using BA.Backend.Domain.Repositories;
using BA.Backend.Application.Common.Interfaces;
using Moq;
using Xunit;
using FluentAssertions;

namespace BA.Backend.Application.Tests.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _mockTenantRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IUserSessionRepository> _mockUserSessionRepository;
    private readonly Mock<ISessionService> _mockSessionService;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _mockTenantRepository = new Mock<ITenantRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserSessionRepository = new Mock<IUserSessionRepository>();
        _mockSessionService = new Mock<ISessionService>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();

        _handler = new LoginCommandHandler(
            _mockTenantRepository.Object,
            _mockUserRepository.Object,
            _mockUserSessionRepository.Object,
            _mockSessionService.Object,
            _mockJwtTokenService.Object,
            _mockPasswordHasher.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnLoginResponse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid().ToString();
        var token = "eyJhbGciOiJIUzI1NiIs...";
        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        var tenant = new Tenant { Id = tenantId, Name = "Admin Tenant", Slug = "admin" };
        var user = new User
        {
            Id = userId,
            TenantId = tenantId,
            Email = "admin@test.com",
            PasswordHash = "$2a$12$ufgNR3HGmE7BZXXS7TvnIe6i2B.k7pCvG7VbFxJJSPNjGqI0l1R7u",
            FullName = "Admin User",
            Role = UserRole.Admin,
            IsActive = true
        };

        var command = new LoginCommand(
            Email: "admin@test.com",
            Password: "Admin123!",
            TenantSlug: "admin",
            DeviceFingerprint: "device-fingerprint-123"
        );

        // Configurar mocks
        _mockTenantRepository
            .Setup(x => x.GetBySlugAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync("admin@test.com", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordHasher
            .Setup(x => x.Verify("Admin123!", user.PasswordHash))
            .Returns(true);

        _mockUserSessionRepository
            .Setup(x => x.GetActiveSessionsByUserAsync(userId))
            .ReturnsAsync(new List<UserSession>());

        _mockJwtTokenService
            .Setup(x => x.GenerateToken(user, tenantId, It.IsAny<string>()))
            .Returns((token, expiresAt));

        _mockUserSessionRepository
            .Setup(x => x.CreateSessionAsync(It.IsAny<UserSession>()))
            .ReturnsAsync(new UserSession { Id = Guid.NewGuid() });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(token);
        result.UserId.Should().Be(userId);
        result.TenantId.Should().Be(tenantId);
        result.UserFullName.Should().Be("Admin User");
        result.ExpiresAt.Should().Be(expiresAt);

        // Verificar que se llamaron los métodos correctos
        _mockTenantRepository.Verify(x => x.GetBySlugAsync("admin", It.IsAny<CancellationToken>()), Times.Once);
        _mockUserRepository.Verify(x => x.GetByEmailAsync("admin@test.com", tenantId, It.IsAny<CancellationToken>()), Times.Once);
        _mockPasswordHasher.Verify(x => x.Verify("Admin123!", user.PasswordHash), Times.Once);
        _mockJwtTokenService.Verify(x => x.GenerateToken(user, tenantId, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidTenant_ShouldThrowException()
    {
        // Arrange
        var command = new LoginCommand(
            Email: "admin@test.com",
            Password: "Admin123!",
            TenantSlug: "invalid-tenant",
            DeviceFingerprint: "device-fingerprint-123"
        );

        _mockTenantRepository
            .Setup(x => x.GetBySlugAsync("invalid-tenant", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Credenciales inválidas");
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldThrowException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tenant = new Tenant { Id = tenantId, Name = "Admin Tenant", Slug = "admin" };
        var user = new User
        {
            Id = userId,
            TenantId = tenantId,
            Email = "admin@test.com",
            PasswordHash = "$2a$12$ufgNR3HGmE7BZXXS7TvnIe6i2B.k7pCvG7VbFxJJSPNjGqI0l1R7u",
            FullName = "Admin User",
            Role = UserRole.Admin,
            IsActive = true
        };

        var command = new LoginCommand(
            Email: "admin@test.com",
            Password: "WrongPassword123!",
            TenantSlug: "admin",
            DeviceFingerprint: "device-fingerprint-123"
        );

        _mockTenantRepository
            .Setup(x => x.GetBySlugAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync("admin@test.com", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordHasher
            .Setup(x => x.Verify("WrongPassword123!", user.PasswordHash))
            .Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("Credenciales inválidas");
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldThrowException()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tenant = new Tenant { Id = tenantId, Name = "Admin Tenant", Slug = "admin" };
        var user = new User
        {
            Id = userId,
            TenantId = tenantId,
            Email = "admin@test.com",
            PasswordHash = "$2a$12$ufgNR3HGmE7BZXXS7TvnIe6i2B.k7pCvG7VbFxJJSPNjGqI0l1R7u",
            FullName = "Admin User",
            Role = UserRole.Admin,
            IsActive = false // ❌ Inactivo
        };

        var command = new LoginCommand(
            Email: "admin@test.com",
            Password: "Admin123!",
            TenantSlug: "admin",
            DeviceFingerprint: "device-fingerprint-123"
        );

        _mockTenantRepository
            .Setup(x => x.GetBySlugAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);

        _mockUserRepository
            .Setup(x => x.GetByEmailAsync("admin@test.com", tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordHasher
            .Setup(x => x.Verify("Admin123!", user.PasswordHash))
            .Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("no está disponible");
    }
}
