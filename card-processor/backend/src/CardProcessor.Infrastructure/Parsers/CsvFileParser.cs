using CardProcessor.Core;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System.Globalization;

namespace CardProcessor.Infrastructure.Parsers;

public class CsvFileParser : IFileParser
{
    public string GetFileExtension() => ".csv";

    public bool CanParseFile(string filePath)
    {
        return Path.GetExtension(filePath).Equals(GetFileExtension(), StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IEnumerable<Transaction>> ParseFileAsync(string filePath)
    {
        var transactions = new List<Transaction>();
        
        try
        {
            var csvContent = await File.ReadAllTextAsync(filePath);
            using var reader = new StringReader(csvContent);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            });

            var records = csv.GetRecords<CsvTransactionRecord>();
            
            foreach (var record in records)
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
                    Console.WriteLine($"Error parsing CSV record: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error parsing CSV file {filePath}: {ex.Message}", ex);
        }

        return transactions;
    }

    private bool TryParseTransaction(CsvTransactionRecord record, out Transaction transaction)
    {
        transaction = null!;

        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(record.CardNumber))
            {
                return false;
            }

            if (!decimal.TryParse(record.Amount, out var amount))
            {
                return false;
            }

            if (!DateTime.TryParse(record.Timestamp, out var timestamp))
            {
                return false;
            }

            // Create transaction (validation happens in constructor)
            transaction = new Transaction(record.CardNumber, amount, timestamp);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public class CsvTransactionRecord
    {
        [Name("cardNumber")]
        public string CardNumber { get; set; } = string.Empty;
        
        [Name("timestamp")]
        public string Timestamp { get; set; } = string.Empty;
        
        [Name("amount")]
        public string Amount { get; set; } = string.Empty;
    }
}


