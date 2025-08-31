using CardProcessor.Application.Services;
using CardProcessor.Core;
using CardProcessor.Infrastructure.Parsers;
using FluentAssertions;
using Moq;

namespace CardProcessor.UnitTests;

public class TransactionProcessorServiceTests
{
    private readonly Mock<IFileParserFactory> _mockFileParserFactory;
    private readonly Mock<ITransactionRepository> _mockTransactionRepository;
    private readonly Mock<IFileParser> _mockFileParser;
    private readonly TransactionProcessorService _service;

    public TransactionProcessorServiceTests()
    {
        _mockFileParserFactory = new Mock<IFileParserFactory>();
        _mockTransactionRepository = new Mock<ITransactionRepository>();
        _mockFileParser = new Mock<IFileParser>();
        
        _service = new TransactionProcessorService(_mockFileParserFactory.Object, _mockTransactionRepository.Object);
    }

    [Fact]
    public async Task ProcessFileAsync_WhenFileExists_ShouldProcessSuccessfully()
    {
        // Arrange
        var filePath = "test.csv";
        var transactions = new List<Transaction>
        {
            new Transaction("4111111111111111", 100.00m, DateTime.UtcNow)
        };

        _mockFileParserFactory.Setup(x => x.CreateParser(filePath)).Returns(_mockFileParser.Object);
        _mockFileParser.Setup(x => x.ParseFileAsync(filePath)).ReturnsAsync(transactions);
        _mockTransactionRepository.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<Transaction>>()))
            .ReturnsAsync((IEnumerable<Transaction> input) => input);

        // Create a real file for testing
        var testFile = "test.csv";
        File.WriteAllText(testFile, "test content");

        try
        {
            // Act
            var result = await _service.ProcessFileAsync(testFile);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.TotalRecords.Should().Be(1);
        }
        finally
        {
            if (File.Exists(testFile))
                File.Delete(testFile);
        }
    }

    [Fact]
    public async Task ProcessFileAsync_WhenFileDoesNotExist_ShouldReturnError()
    {
        // Arrange
        var filePath = "nonexistent.csv";

        // Act
        var result = await _service.ProcessFileAsync(filePath);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be($"File not found: {filePath}");
    }

    [Fact]
    public void ValidateTransactions_ShouldValidateAllTransactions()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new Transaction("4111111111111111", 100.00m, DateTime.UtcNow),
            new Transaction("invalid", 200.00m, DateTime.UtcNow)
        };

        // Act
        var result = _service.ValidateTransactions(transactions);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var resultList = result.ToList();
        resultList[0].IsValid.Should().BeTrue(); // Valid Visa
        resultList[1].IsValid.Should().BeFalse(); // Invalid card number
    }
}
