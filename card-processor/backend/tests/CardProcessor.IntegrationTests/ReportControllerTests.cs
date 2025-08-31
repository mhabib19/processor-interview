using CardProcessor.API;
using CardProcessor.API.DTOs;
using CardProcessor.IntegrationTests.TestHelpers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using System.Net.Http.Headers;

namespace CardProcessor.IntegrationTests;

public class ReportControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ReportControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient().AddJwtToken();
    }

    [Fact]
    public async Task GetByCard_ReturnsAllTransactionsReport()
    {
        // Act
        var response = await _client.GetAsync("/api/report/by-card?page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ReportResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Summary.Should().NotBeNull();
        result.Summary.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        result.Summary.TotalAmount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetByCardType_ReturnsCardTypeReport()
    {
        // Act
        var response = await _client.GetAsync("/api/report/by-card-type?cardType=Visa&page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ReportResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Summary.Should().NotBeNull();
        result.Summary.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        result.Summary.TotalAmount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetByCardType_WithInvalidCardType_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/report/by-card-type?cardType=InvalidType&page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetByDay_ReturnsDateRangeReport()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/api/report/by-day?startDate={startDate}&endDate={endDate}&page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ReportResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Summary.Should().NotBeNull();
        result.Summary.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        result.Summary.TotalAmount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetByDay_WithInvalidDateRange_ReturnsBadRequest()
    {
        // Arrange
        var startDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd"); // End before start

        // Act
        var response = await _client.GetAsync($"/api/report/by-day?startDate={startDate}&endDate={endDate}&page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetRejected_ReturnsRejectedTransactionsReport()
    {
        // Act
        var response = await _client.GetAsync("/api/report/rejected?page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ReportResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Summary.Should().NotBeNull();
        result.Summary.TotalCount.Should().BeGreaterThanOrEqualTo(0);
        result.Summary.TotalAmount.Should().BeGreaterThanOrEqualTo(0);
    }



    [Fact]
    public async Task GetByCard_WithPagination_ReturnsCorrectPage()
    {
        // Act
        var response = await _client.GetAsync("/api/report/by-card?page=2&pageSize=5");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ReportResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Transactions.Should().NotBeNull();
        result.Transactions!.Count().Should().BeLessThanOrEqualTo(5);
    }

    [Fact]
    public async Task GetByCardType_WithAllCardTypes_ReturnsCorrectData()
    {
        // Arrange
        var cardTypes = new[] { "Visa", "MasterCard", "AmericanExpress", "Discover" };

        foreach (var cardType in cardTypes)
        {
            // Act
            var response = await _client.GetAsync($"/api/report/by-card-type?cardType={cardType}&page=1&pageSize=10");

            // Assert
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ReportResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            result.Should().NotBeNull();
            result!.Summary.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task GetByDay_WithFutureDate_ReturnsEmptyResults()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/api/report/by-day?startDate={startDate}&endDate={endDate}&page=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ReportResponse>(responseContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        result.Should().NotBeNull();
        result!.Summary.Should().NotBeNull();
        result.Summary.TotalCount.Should().Be(0);
        result.Summary.TotalAmount.Should().Be(0);
    }

    [Fact]
    public async Task GetByCard_WithInvalidPagination_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/report/by-card?page=0&pageSize=0");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetByCardType_WithInvalidPagination_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/report/by-card-type?cardType=Visa&page=0&pageSize=0");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetByDay_WithInvalidPagination_ReturnsBadRequest()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");
        var endDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Act
        var response = await _client.GetAsync($"/api/report/by-day?startDate={startDate}&endDate={endDate}&page=0&pageSize=0");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetRejected_WithInvalidPagination_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/report/rejected?page=0&pageSize=0");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}


