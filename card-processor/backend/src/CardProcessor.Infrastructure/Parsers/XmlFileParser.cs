using CardProcessor.Core;
using System.Xml;
using System.Xml.Linq;

namespace CardProcessor.Infrastructure.Parsers;

public class XmlFileParser : IFileParser
{
    public string GetFileExtension() => ".xml";

    public bool CanParseFile(string filePath)
    {
        return Path.GetExtension(filePath).Equals(GetFileExtension(), StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IEnumerable<Transaction>> ParseFileAsync(string filePath)
    {
        var transactions = new List<Transaction>();
        
        try
        {
            var xmlContent = await File.ReadAllTextAsync(filePath);
            var doc = XDocument.Parse(xmlContent);
            
            // Look for transaction elements (common patterns)
            var transactionElements = doc.Descendants()
                .Where(e => e.Name.LocalName.Equals("transaction", StringComparison.OrdinalIgnoreCase) ||
                           e.Name.LocalName.Equals("record", StringComparison.OrdinalIgnoreCase) ||
                           e.Name.LocalName.Equals("item", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var element in transactionElements)
            {
                try
                {
                    if (TryParseTransactionElement(element, out var transaction))
                    {
                        transactions.Add(transaction);
                    }
                }
                catch (Exception ex)
                {
                    // Log parsing error for individual record
                    Console.WriteLine($"Error parsing XML record: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error parsing XML file {filePath}: {ex.Message}", ex);
        }

        return transactions;
    }

    private bool TryParseTransactionElement(XElement element, out Transaction transaction)
    {
        transaction = null!;

        try
        {
            // Try to extract card number, timestamp, and amount from various possible element names
            var cardNumber = GetElementValue(element, "cardNumber", "card_number", "card", "number");
            var timestampStr = GetElementValue(element, "timestamp", "date", "time", "created");
            var amountStr = GetElementValue(element, "amount", "value", "sum");

            if (string.IsNullOrWhiteSpace(cardNumber) || 
                string.IsNullOrWhiteSpace(timestampStr) || 
                string.IsNullOrWhiteSpace(amountStr))
            {
                return false;
            }

            if (!decimal.TryParse(amountStr, out var amount))
            {
                return false;
            }

            if (!DateTime.TryParse(timestampStr, out var timestamp))
            {
                return false;
            }

            // Create transaction (validation happens in constructor)
            transaction = new Transaction(cardNumber, amount, timestamp);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private string GetElementValue(XElement parent, params string[] possibleNames)
    {
        foreach (var name in possibleNames)
        {
            var element = parent.Element(name) ?? parent.Element(XName.Get(name, parent.Name.NamespaceName));
            if (element != null)
            {
                return element.Value.Trim();
            }
        }
        return string.Empty;
    }
}


