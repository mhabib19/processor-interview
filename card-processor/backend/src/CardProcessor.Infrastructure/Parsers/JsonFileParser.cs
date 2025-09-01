using CardProcessor.Core;
using System.Text.Json;

namespace CardProcessor.Infrastructure.Parsers;

public class JsonFileParser : IFileParser
{
    public string GetFileExtension() => ".json";

    public bool CanParseFile(string filePath)
    {
        return Path.GetExtension(filePath).Equals(GetFileExtension(), StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IEnumerable<Transaction>> ParseFileAsync(string filePath)
    {
        var transactions = new List<Transaction>();
        
        try
        {
            var jsonContent = await File.ReadAllTextAsync(filePath);
            var jsonTransactions = JsonSerializer.Deserialize<List<JsonTransactionRecord>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (jsonTransactions == null)
            {
                return transactions;
            }

            foreach (var record in jsonTransactions)
            {
                try
                {
                    if (TryParseTransaction(record, out var transaction))
                    {
                        transactions.Add(transaction);
                    }
                }
                catch (Exception ex)
                {
                    // Log parsing error for individual record
                    Console.WriteLine($"Error parsing JSON record: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error parsing JSON file {filePath}: {ex.Message}", ex);
        }

        return transactions;
    }

    private bool TryParseTransaction(JsonTransactionRecord record, out Transaction transaction)
    {
        transaction = null!;

        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(record.CardNumber))
            {
                return false;
            }

            if (record.Amount <= 0)
            {
                return false;
            }

            // Create transaction (validation happens in constructor)
            transaction = new Transaction(record.CardNumber, record.Amount, record.Timestamp);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private class JsonTransactionRecord
    {
        public string CardNumber { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public decimal Amount { get; set; }
    }
}


