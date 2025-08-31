using CardProcessor.API;
using CardProcessor.API.DTOs;
using CardProcessor.IntegrationTests.TestHelpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text;
using System.Text.Json;

namespace CardProcessor.IntegrationTests;

public class TransactionControllerTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly string _testDataDir;

    public TransactionControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient().AddJwtToken();
        
        // Create test data directory and files
        _testDataDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_testDataDir);
        
        // Create a test JSON file
        var jsonData = @"[
            {
                ""cardNumber"": ""4532015112830366"",
                ""timestamp"": ""2024-01-01T10:00:00Z"",
                ""amount"": 100.50
            }
        ]";
        File.WriteAllText(Path.Combine(_testDataDir, "test.json"), jsonData);
    }



    [Fact]
    public async Task GetTransactions_ReturnsTransactionList()
    {
        // Act
        var response = await _client.GetAsync("/api/transaction?page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TransactionListResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Transactions.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetTransactionById_WithValidId_ReturnsTransaction()
    {
        // Arrange - First get a list to find an existing ID
        var listResponse = await _client.GetAsync("/api/transaction?page=1&pageSize=1");
        listResponse.EnsureSuccessStatusCode();
        var listContent = await listResponse.Content.ReadAsStringAsync();
        var listResult = JsonSerializer.Deserialize<TransactionListResponse>(listContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (listResult?.Transactions?.Any() == true)
        {
            var transactionId = listResult.Transactions.First().Id;

            // Act
            var response = await _client.GetAsync($"/api/transaction/{transactionId}");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TransactionDto>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            result.Should().NotBeNull();
            result!.Id.Should().Be(transactionId);
        }
    }

    [Fact]
    public async Task GetTransactionById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/transaction/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRejectedTransactions_ReturnsRejectedTransactions()
    {
        // Act
        var response = await _client.GetAsync("/api/transaction/rejected?page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TransactionListResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Transactions.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThanOrEqualTo(0);
    }



    [Fact]
    public async Task GetTransactions_WithInvalidPagination_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/transaction?page=0&pageSize=0");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }



    public void Dispose()
    {
        // Clean up test data
        if (Directory.Exists(_testDataDir))
        {
            Directory.Delete(_testDataDir, true);
        }
    }
}


