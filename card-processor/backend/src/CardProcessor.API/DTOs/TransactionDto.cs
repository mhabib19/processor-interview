using CardProcessor.Core;

namespace CardProcessor.API.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsValid { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}



public class TransactionListResponse
{
    public List<TransactionDto> Transactions { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class ReportResponse
{
    public ReportSummary Summary { get; set; } = new();
    public List<TransactionDto> Transactions { get; set; } = new();
}

public class ReportSummary
{
    public int TotalCount { get; set; }
    public decimal TotalAmount { get; set; }
    public int ValidTransactions { get; set; }
    public int RejectedTransactions { get; set; }
}



// New DTOs for file upload functionality
public class FileUploadResponse
{
    public bool Success { get; set; }
    public string? FileId { get; set; }
    public string? ErrorMessage { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileType { get; set; } = string.Empty;
}

public class ProcessingStatusResponse
{
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty; // "pending", "processing", "completed", "failed"
    public int TotalRecords { get; set; }
    public int ValidRecords { get; set; }
    public int RejectedRecords { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ProcessingTime { get; set; }
}


