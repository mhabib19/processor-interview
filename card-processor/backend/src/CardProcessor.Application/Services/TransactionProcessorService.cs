using CardProcessor.Core;
using CardProcessor.Infrastructure.Parsers;

namespace CardProcessor.Application.Services;

public class TransactionProcessorService : ITransactionProcessor
{
    private readonly IFileParserFactory _fileParserFactory;
    private readonly ITransactionRepository _transactionRepository;

    public TransactionProcessorService(IFileParserFactory fileParserFactory, ITransactionRepository transactionRepository)
    {
        _fileParserFactory = fileParserFactory;
        _transactionRepository = transactionRepository;
    }

    public async Task<ProcessingResult> ProcessFileAsync(string filePath)
    {
        return await ProcessFileAsync(filePath, false);
    }

    public async Task<ProcessingResult> ProcessFileAsync(string filePath, bool isRealData)
    {
        var result = new ProcessingResult();
        var startTime = DateTime.UtcNow;

        try
        {
            if (!File.Exists(filePath))
            {
                result.Success = false;
                result.ErrorMessage = $"File not found: {filePath}";
                return result;
            }

            // Parse file
            var parser = _fileParserFactory.CreateParser(filePath);
            var transactions = await parser.ParseFileAsync(filePath);
            
            result.TotalRecords = transactions.Count();

            // Process and save ALL transactions (both valid and invalid)
            var validatedTransactions = ValidateTransactions(transactions, isRealData);
            var validTransactions = validatedTransactions.Where(t => t.IsValid).ToList();
            var rejectedTransactions = validatedTransactions.Where(t => !t.IsValid).ToList();

            // Store ALL transactions so they can be queried and displayed
            if (validatedTransactions.Any())
            {
                await _transactionRepository.AddRangeAsync(validatedTransactions);
            }

            result.ValidRecords = validTransactions.Count;
            result.RejectedRecords = rejectedTransactions.Count;
            result.Success = true;

            result.ProcessedFiles.Add(Path.GetFileName(filePath));
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.FailedFiles.Add(Path.GetFileName(filePath));
        }

        result.ProcessingTime = DateTime.UtcNow - startTime;
        return result;
    }

    public IEnumerable<Transaction> ValidateTransactions(IEnumerable<Transaction> transactions)
    {
        return ValidateTransactions(transactions, false);
    }

    public IEnumerable<Transaction> ValidateTransactions(IEnumerable<Transaction> transactions, bool isRealData)
    {
        var validatedTransactions = new List<Transaction>();

        foreach (var transaction in transactions)
        {
            // validate with the real data flag
            transaction.ValidateCard(isRealData);
            validatedTransactions.Add(transaction);
        }

        return validatedTransactions;
    }
}


