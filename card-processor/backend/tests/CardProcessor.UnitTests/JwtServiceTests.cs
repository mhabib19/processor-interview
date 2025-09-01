using CardProcessor.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace CardProcessor.UnitTests;

public class JwtServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        
        _mockConfiguration
            .Setup(x => x["JwtSettings:SecretKey"])
            .Returns("your-super-secret-key-with-at-least-32-characters");
        _mockConfiguration
            .Setup(x => x["JwtSettings:Issuer"])
            .Returns("CardProcessor");
        _mockConfiguration
            .Setup(x => x["JwtSettings:Audience"])
            .Returns("CardProcessorUsers");
        _mockConfiguration
            .Setup(x => x["JwtSettings:ExpirationHours"])
            .Returns("24");

        _jwtService = new JwtService(_mockConfiguration.Object);
    }

    [Fact]
    public void GenerateToken_WithValidInputs_ReturnsValidToken()
    {
        // Arrange
        var username = "testuser";
        var role = "Admin";

        // Act
        var token = _jwtService.GenerateToken(username, role);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().Contain(".");
    }

    [Fact]
    public void ValidateToken_WithValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var username = "testuser";
        var role = "Admin";
        var token = _jwtService.GenerateToken(username, role);

        // Act
        var principal = _jwtService.ValidateToken(token);

        // Assert
        principal.Should().NotBeNull();
        principal!.Identity!.Name.Should().Be(username);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var principal = _jwtService.ValidateToken(invalidToken);

        // Assert
        principal.Should().BeNull();
    }

    [Fact]
    public void GenerateToken_WithMissingSecretKey_ThrowsException()
    {
        // Arrange
        _mockConfiguration
            .Setup(x => x["JwtSettings:SecretKey"])
            .Returns((string?)null);
        var username = "testuser";
        var role = "Admin";

        // Act & Assert
        var action = () => _jwtService.GenerateToken(username, role);
        action.Should().Throw<InvalidOperationException>();
    }
}
