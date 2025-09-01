using CardProcessor.API.DTOs;
using CardProcessor.Application.Services;
using CardProcessor.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardProcessor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly ITransactionProcessor _transactionProcessor;
    private readonly ITransactionRepository _transactionRepository;

    public TransactionController(ITransactionProcessor transactionProcessor, ITransactionRepository transactionRepository)
    {
        _transactionProcessor = transactionProcessor;
        _transactionRepository = transactionRepository;
    }


    [HttpGet]
    public async Task<ActionResult<TransactionListResponse>> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? cardType = null,
        [FromQuery] bool? isValid = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        try
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest(new { error = "Page and page size must be greater than 0" });
            }

            // Log the received parameters for debugging
            Console.WriteLine($"Received filters - CardType: {cardType}, IsValid: {isValid}, DateFrom: {dateFrom}, DateTo: {dateTo}");

            // Get all transactions and apply filters in memory for now
            // In a production environment, you'd want to implement proper database queries
            var allTransactions = await _transactionRepository.GetAllAsync(1, int.MaxValue);
            
            // Apply filters
            var filteredTransactions = allTransactions.AsEnumerable();
            
            if (!string.IsNullOrEmpty(cardType) && Enum.TryParse<CardType>(cardType, true, out var parsedCardType))
            {
                filteredTransactions = filteredTransactions.Where(t => t.CardType == parsedCardType);
                Console.WriteLine($"Applied card type filter: {parsedCardType}");
            }
            
            if (isValid.HasValue)
            {
                filteredTransactions = filteredTransactions.Where(t => t.IsValid == isValid.Value);
                Console.WriteLine($"Applied status filter: {isValid.Value}");
            }
            
            if (dateFrom.HasValue)
            {
                filteredTransactions = filteredTransactions.Where(t => t.Timestamp.Date >= dateFrom.Value.Date);
                Console.WriteLine($"Applied date from filter: {dateFrom.Value.Date}");
            }
            
            if (dateTo.HasValue)
            {
                filteredTransactions = filteredTransactions.Where(t => t.Timestamp.Date <= dateTo.Value.Date);
                Console.WriteLine($"Applied date to filter: {dateTo.Value.Date}");
            }
            
            // Apply pagination
            var totalCount = filteredTransactions.Count();
            var paginatedTransactions = filteredTransactions
                .OrderByDescending(t => t.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            Console.WriteLine($"Total filtered transactions: {totalCount}, Returning: {paginatedTransactions.Count}");

            var transactionDtos = paginatedTransactions.Select(MapToDto).ToList();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var response = new TransactionListResponse
            {
                Transactions = transactionDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetTransactions: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionDto>> GetTransaction(Guid id)
    {
        try
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            return Ok(MapToDto(transaction));
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("rejected")]
    public async Task<ActionResult<TransactionListResponse>> GetRejectedTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest(new { error = "Page and page size must be greater than 0" });
            }
            var transactions = await _transactionRepository.GetRejectedTransactionsAsync(page, pageSize);
            var totalCount = await _transactionRepository.GetRejectedCountAsync();

            var transactionDtos = transactions.Select(MapToDto).ToList();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var response = new TransactionListResponse
            {
                Transactions = transactionDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private static TransactionDto MapToDto(Transaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            CardNumber = transaction.GetMaskedCardNumber(),
            CardType = transaction.CardType.GetDisplayName(),
            Amount = transaction.Amount,
            Timestamp = transaction.Timestamp,
            IsValid = transaction.IsValid,
            RejectionReason = transaction.RejectionReason,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt
        };
    }
}


