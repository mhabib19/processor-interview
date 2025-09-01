using CardProcessor.Core;

namespace CardProcessor.UnitTests;

public class CardValidatorTests
{
    [Theory]
    [InlineData("4532015112830366")] // Valid Visa
    [InlineData("5555555555554444")] // Valid MasterCard
    [InlineData("378282246310005")]  // Valid American Express
    [InlineData("6011111111111117")] // Valid Discover
    [InlineData("4111111111111111")] // Valid Visa
    [InlineData("5105105105105100")] // Valid MasterCard
    public void IsValidCardNumber_WithValidCards_ReturnsTrue(string cardNumber)
    {
        // Act
        var result = CardValidator.IsValidCardNumber(cardNumber);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("4532015112830367")] // Invalid Visa (last digit changed)
    [InlineData("5555555555554445")] // Invalid MasterCard (last digit changed)
    [InlineData("378282246310006")]  // Invalid American Express (last digit changed)
    [InlineData("6011111111111118")] // Invalid Discover (last digit changed)
    [InlineData("4111111111111112")] // Invalid Visa (last digit changed)
    [InlineData("5105105105105101")] // Invalid MasterCard (last digit changed)
    public void IsValidCardNumber_WithInvalidCards_ReturnsFalse(string cardNumber)
    {
        // Act
        var result = CardValidator.IsValidCardNumber(cardNumber);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void IsValidCardNumber_WithEmptyOrNullInput_ReturnsFalse(string cardNumber)
    {
        // Act
        var result = CardValidator.IsValidCardNumber(cardNumber);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("123")] // Too short
    [InlineData("12345678901234567890")] // Too long
    [InlineData("123456789012")] // Too short
    public void IsValidCardNumber_WithInvalidLength_ReturnsFalse(string cardNumber)
    {
        // Act
        var result = CardValidator.IsValidCardNumber(cardNumber);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("4532015112830366a")] // Contains letter
    [InlineData("4532015112830366-")] // Contains dash
    [InlineData("4532015112830366 ")] // Contains space
    public void IsValidCardNumber_WithNonNumericCharacters_ReturnsFalse(string cardNumber)
    {
        // Act
        var result = CardValidator.IsValidCardNumber(cardNumber);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("4532015112830366", "****-****-****-0366")]
    [InlineData("5555555555554444", "****-****-****-4444")]
    [InlineData("378282246310005", "****-****-****-0005")]
    [InlineData("4111111111111111", "****-****-****-1111")]
    public void MaskCardNumber_WithValidCards_ReturnsMaskedNumber(string cardNumber, string expectedMasked)
    {
        // Act
        var result = CardValidator.MaskCardNumber(cardNumber);

        // Assert
        result.Should().Be(expectedMasked);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void MaskCardNumber_WithEmptyOrNullInput_ReturnsEmptyString(string cardNumber)
    {
        // Act
        var result = CardValidator.MaskCardNumber(cardNumber);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("123")]
    [InlineData("12")]
    [InlineData("1")]
    public void MaskCardNumber_WithShortInput_ReturnsOriginal(string cardNumber)
    {
        // Act
        var result = CardValidator.MaskCardNumber(cardNumber);

        // Assert
        result.Should().Be(cardNumber);
    }

    [Theory]
    [InlineData("4532015112830366", "****-****-****-0366")]
    [InlineData("4532 0151 1283 0366", "****-****-****-0366")] // With spaces
    [InlineData("4532-0151-1283-0366", "****-****-****-0366")] // With dashes
    public void MaskCardNumber_WithFormattedInput_ReturnsMaskedNumber(string cardNumber, string expectedMasked)
    {
        // Act
        var result = CardValidator.MaskCardNumber(cardNumber);

        // Assert
        result.Should().Be(expectedMasked);
    }
}


