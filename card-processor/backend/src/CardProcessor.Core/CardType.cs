namespace CardProcessor.Core;

public enum CardType
{
    Visa,
    MasterCard,
    AmericanExpress,
    Discover,
    Unknown
}

public static class CardTypeExtensions
{
    public static CardType DetectCardType(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return CardType.Unknown;
        }

        // Remove any spaces or dashes
        var cleanNumber = cardNumber.Replace(" ", "").Replace("-", "");
        
        if (cleanNumber.Length < 13 || cleanNumber.Length > 19)
        {
            return CardType.Unknown;
        }

        // Visa: starts with 4, length 13, 16, or 19
        if (cleanNumber.StartsWith("4") && (cleanNumber.Length == 13 || cleanNumber.Length == 16 || cleanNumber.Length == 19))
        {
            return CardType.Visa;
        }

        // MasterCard: starts with 51-55 or 2221-2720, length 16
        if (cleanNumber.Length == 16)
        {
            var prefix = int.Parse(cleanNumber.Substring(0, 2));
            if (prefix >= 51 && prefix <= 55)
            {
                return CardType.MasterCard;
            }

            var fourDigitPrefix = int.Parse(cleanNumber.Substring(0, 4));
            if (fourDigitPrefix >= 2221 && fourDigitPrefix <= 2720)
            {
                return CardType.MasterCard;
            }
        }

        // American Express: starts with 34 or 37, length 15
        if (cleanNumber.Length == 15 && (cleanNumber.StartsWith("34") || cleanNumber.StartsWith("37")))
        {
            return CardType.AmericanExpress;
        }

        // Discover: starts with 6011, 622126-622925, 644-649, 65, length 16
        if (cleanNumber.Length == 16)
        {
            if (cleanNumber.StartsWith("6011"))
            {
                return CardType.Discover;
            }

            if (cleanNumber.StartsWith("65"))
            {
                return CardType.Discover;
            }

            var threeDigitPrefix = int.Parse(cleanNumber.Substring(0, 3));
            if (threeDigitPrefix >= 644 && threeDigitPrefix <= 649)
            {
                return CardType.Discover;
            }

            var sixDigitPrefix = int.Parse(cleanNumber.Substring(0, 6));
            if (sixDigitPrefix >= 622126 && sixDigitPrefix <= 622925)
            {
                return CardType.Discover;
            }
        }

        return CardType.Unknown;
    }

    public static string GetDisplayName(this CardType cardType)
    {
        return cardType switch
        {
            CardType.Visa => "Visa",
            CardType.MasterCard => "MasterCard",
            CardType.AmericanExpress => "American Express",
            CardType.Discover => "Discover",
            CardType.Unknown => "Unknown",
            _ => "Unknown"
        };
    }
}


