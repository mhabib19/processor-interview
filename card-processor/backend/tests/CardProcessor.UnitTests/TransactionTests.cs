using CardProcessor.Core;

namespace CardProcessor.UnitTests;

public class TransactionTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesValidTransaction()
    {
        // Arrange
        var cardNumber = "4532015112830366";
        var amount = 100.50m;
        var timestamp = DateTime.UtcNow;

        // Act
        var transaction = new Transaction(cardNumber, amount, timestamp);
        transaction.ValidateCard(false); // Explicitly validate

        // Assert
        transaction.CardNumber.Should().Be(cardNumber);
        transaction.Amount.Should().Be(amount);
        transaction.Timestamp.Should().Be(timestamp);
        transaction.IsValid.Should().BeTrue();
        transaction.RejectionReason.Should().BeNull();
        transaction.CardType.Should().Be(CardType.Visa);
        transaction.Id.Should().NotBeEmpty();
        transaction.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        transaction.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_WithInvalidCardNumber_CreatesInvalidTransaction()
    {
        // Arrange
        var cardNumber = "4532015112830367"; // Invalid Luhn
        var amount = 100.50m;
        var timestamp = DateTime.UtcNow;

        // Act
        var transaction = new Transaction(cardNumber, amount, timestamp);
        // Force Luhn check by setting isRealData to true
        transaction.ValidateCard(true);

        // Assert
        transaction.IsValid.Should().BeFalse();
        transaction.RejectionReason.Should().Be("Invalid card number (failed Luhn algorithm check)");
        transaction.CardType.Should().Be(CardType.Unknown);
    }

    [Fact]
    public void Constructor_WithUnknownCardType_CreatesInvalidTransaction()
    {
        // Arrange
        var cardNumber = "1234567890123456"; // Unknown pattern
        var amount = 100.50m;
        var timestamp = DateTime.UtcNow;

        // Act
        var transaction = new Transaction(cardNumber, amount, timestamp);
        transaction.ValidateCard(false); // Explicitly validate

        // Assert
        transaction.IsValid.Should().BeFalse();
        transaction.RejectionReason.Should().Be("Unknown card type");
        transaction.CardType.Should().Be(CardType.Unknown);
    }

    [Fact]
    public void Constructor_WithNonNumericCardNumber_CreatesInvalidTransaction()
    {
        // Arrange
        var cardNumber = "4532015112830366a"; // Contains letter
        var amount = 100.50m;
        var timestamp = DateTime.UtcNow;

        // Act
        var transaction = new Transaction(cardNumber, amount, timestamp);
        transaction.ValidateCard(false); // Explicitly validate

        // Assert
        transaction.IsValid.Should().BeFalse();
        transaction.RejectionReason.Should().Be("Card number contains non-numeric characters");
    }

    [Theory]
    [InlineData("4532015112830366", "****-****-****-0366")]
    [InlineData("5555555555554444", "****-****-****-4444")]
    [InlineData("378282246310005", "****-****-****-0005")]
    public void GetMaskedCardNumber_WithValidCards_ReturnsMaskedNumber(string cardNumber, string expectedMasked)
    {
        // Arrange
        var transaction = new Transaction(cardNumber, 100.50m, DateTime.UtcNow);

        // Act
        var result = transaction.GetMaskedCardNumber();

        // Assert
        result.Should().Be(expectedMasked);
    }

    [Fact]
    public void Update_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var transaction = new Transaction("4532015112830366", 100.50m, DateTime.UtcNow);
        transaction.ValidateCard(false); // Explicitly validate
        var originalUpdatedAt = transaction.UpdatedAt;

        // Act
        Thread.Sleep(10); // Ensure time difference
        transaction.Update();

        // Assert
        transaction.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Theory]
    [InlineData("4532015112830366", CardType.Visa)]
    [InlineData("5555555555554444", CardType.MasterCard)]
    [InlineData("378282246310005", CardType.AmericanExpress)]
    [InlineData("6011111111111117", CardType.Discover)]
    public void Constructor_WithDifferentCardTypes_SetsCorrectCardType(string cardNumber, CardType expectedType)
    {
        // Act
        var transaction = new Transaction(cardNumber, 100.50m, DateTime.UtcNow);
        transaction.ValidateCard(false); // Explicitly validate

        // Assert
        transaction.CardType.Should().Be(expectedType);
        transaction.IsValid.Should().BeTrue();
    }
}
