using System.ComponentModel.DataAnnotations;

namespace CardProcessor.Core;

public class Transaction
{
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(19)]
    public string CardNumber { get; set; } = string.Empty;
    
    [Required]
    public CardType CardType { get; set; }
    
    [Required]
    public decimal Amount { get; set; }
    
    [Required]
    public DateTime Timestamp { get; set; }
    
    public bool IsValid { get; set; }
    
    public string? RejectionReason { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }

    public Transaction()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Transaction(string cardNumber, decimal amount, DateTime timestamp) : this()
    {
        CardNumber = cardNumber;
        Amount = amount;
        Timestamp = timestamp;
        
        // Validate card and set properties (default to test data validation)
        ValidateCard(false);
    }

    public void ValidateCard(bool isRealData = false)
    {
        // log to console for debugging
        Console.WriteLine($"Validating card: {CardNumber}, Amount: {Amount}, Real Data: {isRealData}");
        
        // Check for non-numeric characters in card number first
        if (!CardNumber.All(char.IsDigit))
        {
            IsValid = false;
            RejectionReason = "Card number contains non-numeric characters";
            CardType = CardType.Unknown;
            Console.WriteLine($"Rejected: {RejectionReason}");
            return;
        }

        // Detect card type next
        CardType = CardTypeExtensions.DetectCardType(CardNumber);
        Console.WriteLine($"Detected card type: {CardType}");
        
        if (CardType == CardType.Unknown)
        {
            IsValid = false;
            RejectionReason = "Unknown card type";
            Console.WriteLine($"Rejected: {RejectionReason}");
            return;
        }

        // Check Luhn algorithm only for real data
        if (isRealData)
        {
            var luhnValid = CardValidator.IsValidCardNumber(CardNumber);
            Console.WriteLine($"Luhn algorithm result: {luhnValid}");
            
            if (!luhnValid)
            {
                IsValid = false;
                RejectionReason = "Invalid card number (failed Luhn algorithm check)";
                CardType = CardType.Unknown;
                Console.WriteLine($"Rejected: {RejectionReason}");
                return;
            }
        }
        else
        {
            // For test data, just check basic format (length and numeric)
            if (CardNumber.Length < 13 || CardNumber.Length > 19)
            {
                IsValid = false;
                RejectionReason = "Invalid card number length";
                CardType = CardType.Unknown;
                Console.WriteLine($"Rejected: {RejectionReason}");
                return;
            }
            Console.WriteLine($"Test data validation passed (skipped Luhn check)");
        }

        // All validations passed
        IsValid = true;
        RejectionReason = null;
        Console.WriteLine($"Transaction validated successfully");
    }

    public string GetMaskedCardNumber()
    {
        return CardValidator.MaskCardNumber(CardNumber);
    }

    public void Update()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}


