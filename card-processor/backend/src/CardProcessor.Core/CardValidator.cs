namespace CardProcessor.Core;

public static class CardValidator
{
    public static bool IsValidCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return false;
        }

        // Check if it contains only digits (no spaces, dashes, or other characters)
        if (!cardNumber.All(char.IsDigit))
        {
            return false;
        }
            
        // Check length (most cards are 13-19 digits)
        if (cardNumber.Length < 13 || cardNumber.Length > 19)
        {
            return false;
        }

        // Apply Luhn algorithm
        return IsLuhnValid(cardNumber);
    }

    private static bool IsLuhnValid(string cardNumber)
    {
        var sum = 0;
        var isEven = false;

        // Process from right to left
        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            var digit = int.Parse(cardNumber[i].ToString());

            if (isEven)
            {
                digit *= 2;
                if (digit > 9)
                {
                    digit = digit % 10 + 1;
                }
            }

            sum += digit;
            isEven = !isEven;
        }

        return sum % 10 == 0;
    }

    public static string MaskCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return string.Empty;
        }

        var cleanNumber = cardNumber.Replace(" ", "").Replace("-", "");
        
        if (cleanNumber.Length < 4)
        {
            return cleanNumber;
        }

        // Show only last 4 digits
        return $"****-****-****-{cleanNumber.Substring(cleanNumber.Length - 4)}";
    }
}


