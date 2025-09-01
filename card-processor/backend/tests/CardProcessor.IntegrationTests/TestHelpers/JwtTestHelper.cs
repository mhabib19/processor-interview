using CardProcessor.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace CardProcessor.IntegrationTests.TestHelpers;

public static class JwtTestHelper
{
    public static string GenerateTestToken()
    {
        // Create a test configuration with the same JWT settings as the app
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["JwtSettings:SecretKey"] = "your-super-secret-key-with-at-least-32-characters-for-jwt-signing",
                ["JwtSettings:Issuer"] = "CardProcessor",
                ["JwtSettings:Audience"] = "CardProcessorAPI",
                ["JwtSettings:ExpirationHours"] = "24"
            })
            .Build();

        var jwtService = new JwtService(configuration);
        return jwtService.GenerateToken("test-user", "TestRole");
    }

    public static HttpClient AddJwtToken(this HttpClient client)
    {
        var token = GenerateTestToken();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}

