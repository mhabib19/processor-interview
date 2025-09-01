namespace CardProcessor.Core;

public interface ITransactionProcessor
{
    Task<ProcessingResult> ProcessFileAsync(string filePath);
    Task<ProcessingResult> ProcessFileAsync(string filePath, bool isRealData);
    IEnumerable<Transaction> ValidateTransactions(IEnumerable<Transaction> transactions);
    IEnumerable<Transaction> ValidateTransactions(IEnumerable<Transaction> transactions, bool isRealData);
}

public class ProcessingResult
{
    public bool Success { get; set; }
    public int TotalRecords { get; set; }
    public int ValidRecords { get; set; }
    public int RejectedRecords { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> ProcessedFiles { get; set; } = new();
    public List<string> FailedFiles { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
}


