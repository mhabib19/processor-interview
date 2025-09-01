using CardProcessor.Core;

namespace CardProcessor.UnitTests;

public class CardTypeExtensionsTests
{
    [Theory]
    [InlineData("4532015112830366", CardType.Visa)] // 16 digits
    [InlineData("4532015112834", CardType.Visa)]    // 13 digits
    [InlineData("4532015112830366123", CardType.Visa)] // 19 digits
    [InlineData("4111111111111111", CardType.Visa)]
    [InlineData("4005550000000019", CardType.Visa)]
    public void DetectCardType_WithVisaCards_ReturnsVisa(string cardNumber, CardType expectedType)
    {
        // Act
        var result = CardTypeExtensions.DetectCardType(cardNumber);

        // Assert
        result.Should().Be(expectedType);
    }

    [Theory]
    [InlineData("5555555555554444", CardType.MasterCard)] // 51-55 range
    [InlineData("5105105105105100", CardType.MasterCard)]
    [InlineData("2221000000000009", CardType.MasterCard)] // 2221-2720 range
    [InlineData("2720990000000000", CardType.MasterCard)]
    [InlineData("2223000048400011", CardType.MasterCard)]
    public void DetectCardType_WithMasterCardCards_ReturnsMasterCard(string cardNumber, CardType expectedType)
    {
        // Act
        var result = CardTypeExtensions.DetectCardType(cardNumber);

        // Assert
        result.Should().Be(expectedType);
    }

    [Theory]
    [InlineData("378282246310005", CardType.AmericanExpress)] // 34
    [InlineData("371449635398431", CardType.AmericanExpress)] // 37
    [InlineData("378734493671000", CardType.AmericanExpress)]
    public void DetectCardType_WithAmericanExpressCards_ReturnsAmericanExpress(string cardNumber, CardType expectedType)
    {
        // Act
        var result = CardTypeExtensions.DetectCardType(cardNumber);

        // Assert
        result.Should().Be(expectedType);
    }

    [Theory]
    [InlineData("6011111111111117", CardType.Discover)] // 6011
    [InlineData("6500000000000002", CardType.Discover)] // 65
    [InlineData("6440000000000005", CardType.Discover)] // 644-649 range
    [InlineData("6490000000000008", CardType.Discover)]
    [InlineData("6221260000000000", CardType.Discover)] // 622126-622925 range
    [InlineData("6229250000000000", CardType.Discover)]
    public void DetectCardType_WithDiscoverCards_ReturnsDiscover(string cardNumber, CardType expectedType)
    {
        // Act
        var result = CardTypeExtensions.DetectCardType(cardNumber);

        // Assert
        result.Should().Be(expectedType);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void DetectCardType_WithEmptyOrNullInput_ReturnsUnknown(string cardNumber)
    {
        // Act
        var result = CardTypeExtensions.DetectCardType(cardNumber);

        // Assert
        result.Should().Be(CardType.Unknown);
    }

    [Theory]
    [InlineData("123")] // Too short
    [InlineData("12345678901234567890")] // Too long
    [InlineData("123456789012")] // Too short
    public void DetectCardType_WithInvalidLength_ReturnsUnknown(string cardNumber)
    {
        // Act
        var result = CardTypeExtensions.DetectCardType(cardNumber);

        // Assert
        result.Should().Be(CardType.Unknown);
    }

    [Theory]
    [InlineData("1234567890123456")] // Unknown pattern
    [InlineData("9999999999999999")] // Unknown pattern
    [InlineData("123456789012345")] // Unknown pattern
    public void DetectCardType_WithUnknownPatterns_ReturnsUnknown(string cardNumber)
    {
        // Act
        var result = CardTypeExtensions.DetectCardType(cardNumber);

        // Assert
        result.Should().Be(CardType.Unknown);
    }

    [Theory]
    [InlineData("4532 0151 1283 0366", CardType.Visa)] // With spaces
    [InlineData("4532-0151-1283-0366", CardType.Visa)] // With dashes
    [InlineData("5555 5555 5555 4444", CardType.MasterCard)] // With spaces
    [InlineData("3782 822463 10005", CardType.AmericanExpress)] // With spaces
    public void DetectCardType_WithFormattedInput_DetectsCorrectly(string cardNumber, CardType expectedType)
    {
        // Act
        var result = CardTypeExtensions.DetectCardType(cardNumber);

        // Assert
        result.Should().Be(expectedType);
    }

    [Theory]
    [InlineData(CardType.Visa, "Visa")]
    [InlineData(CardType.MasterCard, "MasterCard")]
    [InlineData(CardType.AmericanExpress, "American Express")]
    [InlineData(CardType.Discover, "Discover")]
    [InlineData(CardType.Unknown, "Unknown")]
    public void GetDisplayName_WithAllCardTypes_ReturnsCorrectName(CardType cardType, string expectedName)
    {
        // Act
        var result = cardType.GetDisplayName();

        // Assert
        result.Should().Be(expectedName);
    }
}


