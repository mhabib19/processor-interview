using CardProcessor.Core;
using CardProcessor.Infrastructure.Parsers;
using System.Text;

namespace CardProcessor.UnitTests;

public class FileParserTests
{
    private readonly string _testDataPath = Path.Combine(Path.GetTempPath(), "test_data");

    public FileParserTests()
    {
        // Ensure test directory exists
        Directory.CreateDirectory(_testDataPath);
    }

    [Fact]
    public async Task CsvFileParser_WithValidCsvFile_ParsesCorrectly()
    {
        // Arrange
        var csvContent = @"cardNumber,timestamp,amount
4532015112830366,2024-01-01T10:00:00Z,100.50
5555555555554444,2024-01-01T11:00:00Z,200.75";
        
        var filePath = Path.Combine(_testDataPath, "test.csv");
        await File.WriteAllTextAsync(filePath, csvContent);
        
        var parser = new CsvFileParser();

        // Act
        var transactions = await parser.ParseFileAsync(filePath);

        // Assert
        transactions.Should().HaveCount(2);
        var firstTransaction = transactions.First();
        firstTransaction.CardNumber.Should().Be("4532015112830366");
        firstTransaction.Amount.Should().Be(100.50m);
        firstTransaction.CardType.Should().Be(CardType.Visa);
        firstTransaction.IsValid.Should().BeTrue();

        var secondTransaction = transactions.Last();
        secondTransaction.CardNumber.Should().Be("5555555555554444");
        secondTransaction.Amount.Should().Be(200.75m);
        secondTransaction.CardType.Should().Be(CardType.MasterCard);
        secondTransaction.IsValid.Should().BeTrue();

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public async Task JsonFileParser_WithValidJsonFile_ParsesCorrectly()
    {
        // Arrange
        var jsonContent = @"[
  {
    ""cardNumber"": ""4532015112830366"",
    ""timestamp"": ""2024-01-01T10:00:00Z"",
    ""amount"": 100.50
  },
  {
    ""cardNumber"": ""5555555555554444"",
    ""timestamp"": ""2024-01-01T11:00:00Z"",
    ""amount"": 200.75
  }
]";
        
        var filePath = Path.Combine(_testDataPath, "test.json");
        await File.WriteAllTextAsync(filePath, jsonContent);
        
        var parser = new JsonFileParser();

        // Act
        var transactions = await parser.ParseFileAsync(filePath);

        // Assert
        transactions.Should().HaveCount(2);
        var firstTransaction = transactions.First();
        firstTransaction.CardNumber.Should().Be("4532015112830366");
        firstTransaction.Amount.Should().Be(100.50m);
        firstTransaction.CardType.Should().Be(CardType.Visa);
        firstTransaction.IsValid.Should().BeTrue();

        var secondTransaction = transactions.Last();
        secondTransaction.CardNumber.Should().Be("5555555555554444");
        secondTransaction.Amount.Should().Be(200.75m);
        secondTransaction.CardType.Should().Be(CardType.MasterCard);
        secondTransaction.IsValid.Should().BeTrue();

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public async Task XmlFileParser_WithValidXmlFile_ParsesCorrectly()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<transactions>
  <transaction>
    <cardNumber>4532015112830366</cardNumber>
    <timestamp>2024-01-01T10:00:00Z</timestamp>
    <amount>100.50</amount>
  </transaction>
  <transaction>
    <cardNumber>5555555555554444</cardNumber>
    <timestamp>2024-01-01T11:00:00Z</timestamp>
    <amount>200.75</amount>
  </transaction>
</transactions>";
        
        var filePath = Path.Combine(_testDataPath, "test.xml");
        await File.WriteAllTextAsync(filePath, xmlContent);
        
        var parser = new XmlFileParser();

        // Act
        var transactions = await parser.ParseFileAsync(filePath);

        // Assert
        transactions.Should().HaveCount(2);
        var firstTransaction = transactions.First();
        firstTransaction.CardNumber.Should().Be("4532015112830366");
        firstTransaction.Amount.Should().Be(100.50m);
        firstTransaction.CardType.Should().Be(CardType.Visa);
        firstTransaction.IsValid.Should().BeTrue();

        var secondTransaction = transactions.Last();
        secondTransaction.CardNumber.Should().Be("5555555555554444");
        secondTransaction.Amount.Should().Be(200.75m);
        secondTransaction.CardType.Should().Be(CardType.MasterCard);
        secondTransaction.IsValid.Should().BeTrue();

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public async Task CsvFileParser_WithInvalidCardNumbers_CreatesInvalidTransactions()
    {
        // Arrange
        var csvContent = @"cardNumber,timestamp,amount
9999999999999999,2024-01-01T10:00:00Z,100.50
1234567890123456,2024-01-01T11:00:00Z,200.75";
        
        var filePath = Path.Combine(_testDataPath, "invalid.csv");
        await File.WriteAllTextAsync(filePath, csvContent);
        
        var parser = new CsvFileParser();

        // Act
        var transactions = await parser.ParseFileAsync(filePath);

        // Assert
        transactions.Should().HaveCount(2);
        transactions.Should().OnlyContain(t => !t.IsValid);
        transactions.All(t => t.RejectionReason == "Unknown card type").Should().BeTrue();

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public async Task JsonFileParser_WithInvalidJson_ThrowsException()
    {
        // Arrange
        var invalidJson = "{ invalid json content";
        var filePath = Path.Combine(_testDataPath, "invalid.json");
        await File.WriteAllTextAsync(filePath, invalidJson);
        
        var parser = new JsonFileParser();

        // Act & Assert
        var action = () => parser.ParseFileAsync(filePath);
        await action.Should().ThrowAsync<Exception>();

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public async Task XmlFileParser_WithInvalidXml_ThrowsException()
    {
        // Arrange
        var invalidXml = "<invalid xml content";
        var filePath = Path.Combine(_testDataPath, "invalid.xml");
        await File.WriteAllTextAsync(filePath, invalidXml);
        
        var parser = new XmlFileParser();

        // Act & Assert
        var action = () => parser.ParseFileAsync(filePath);
        await action.Should().ThrowAsync<Exception>();

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void CanParseFile_WithCorrectExtensions_ReturnsTrue()
    {
        // Arrange
        var csvParser = new CsvFileParser();
        var jsonParser = new JsonFileParser();
        var xmlParser = new XmlFileParser();

        // Act & Assert
        csvParser.CanParseFile("test.csv").Should().BeTrue();
        csvParser.CanParseFile("test.CSV").Should().BeTrue();
        csvParser.CanParseFile("test.txt").Should().BeFalse();

        jsonParser.CanParseFile("test.json").Should().BeTrue();
        jsonParser.CanParseFile("test.JSON").Should().BeTrue();
        jsonParser.CanParseFile("test.txt").Should().BeFalse();

        xmlParser.CanParseFile("test.xml").Should().BeTrue();
        xmlParser.CanParseFile("test.XML").Should().BeTrue();
        xmlParser.CanParseFile("test.txt").Should().BeFalse();
    }

    [Fact]
    public void GetFileExtension_ReturnsCorrectExtensions()
    {
        // Arrange
        var csvParser = new CsvFileParser();
        var jsonParser = new JsonFileParser();
        var xmlParser = new XmlFileParser();

        // Act & Assert
        csvParser.GetFileExtension().Should().Be(".csv");
        jsonParser.GetFileExtension().Should().Be(".json");
        xmlParser.GetFileExtension().Should().Be(".xml");
    }

    [Fact]
    public void FileParserFactory_WithCsvFile_ReturnsCsvParser()
    {
        // Arrange
        var factory = new FileParserFactory();

        // Act
        var parser = factory.CreateParser("test.csv");

        // Assert
        parser.Should().BeOfType<CsvFileParser>();
    }

    [Fact]
    public void FileParserFactory_WithJsonFile_ReturnsJsonParser()
    {
        // Arrange
        var factory = new FileParserFactory();

        // Act
        var parser = factory.CreateParser("test.json");

        // Assert
        parser.Should().BeOfType<JsonFileParser>();
    }

    [Fact]
    public void FileParserFactory_WithXmlFile_ReturnsXmlParser()
    {
        // Arrange
        var factory = new FileParserFactory();

        // Act
        var parser = factory.CreateParser("test.xml");

        // Assert
        parser.Should().BeOfType<XmlFileParser>();
    }

    [Fact]
    public void FileParserFactory_WithUnsupportedFile_ThrowsException()
    {
        // Arrange
        var factory = new FileParserFactory();

        // Act & Assert
        var action = () => factory.CreateParser("test.txt");
        action.Should().Throw<NotSupportedException>();
    }


}
