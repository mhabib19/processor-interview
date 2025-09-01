using CardProcessor.API.DTOs;
using CardProcessor.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CardProcessor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly ITransactionRepository _transactionRepository;

    public ReportController(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    [HttpGet("by-card")]
    public async Task<ActionResult<ReportResponse>> GetReportByCard([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest(new { error = "Page and page size must be greater than 0" });
            }
            var transactions = await _transactionRepository.GetAllAsync(page, pageSize);
            var totalCount = await _transactionRepository.GetTotalCountAsync();
            var totalAmount = await _transactionRepository.GetTotalAmountAsync();
            var rejectedCount = await _transactionRepository.GetRejectedCountAsync();

            var transactionDtos = transactions.Select(MapToDto).ToList();

            var response = new ReportResponse
            {
                Summary = new ReportSummary
                {
                    TotalCount = totalCount,
                    TotalAmount = totalAmount,
                    ValidTransactions = totalCount - rejectedCount,
                    RejectedTransactions = rejectedCount
                },
                Transactions = transactionDtos
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("by-card-type")]
    public async Task<ActionResult<ReportResponse>> GetReportByCardType([FromQuery] string cardType, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest(new { error = "Page and page size must be greater than 0" });
            }
            if (!Enum.TryParse<CardType>(cardType, true, out var parsedCardType))
            {
                return BadRequest(new { error = "Invalid card type" });
            }

            var transactions = await _transactionRepository.GetByCardTypeAsync(parsedCardType, page, pageSize);
            var totalCount = await _transactionRepository.GetCountByCardTypeAsync(parsedCardType);
            var totalAmount = await _transactionRepository.GetTotalAmountByCardTypeAsync(parsedCardType);

            var transactionDtos = transactions.Select(MapToDto).ToList();

            var response = new ReportResponse
            {
                Summary = new ReportSummary
                {
                    TotalCount = totalCount,
                    TotalAmount = totalAmount,
                    ValidTransactions = totalCount, // Assuming all transactions in card type are valid
                    RejectedTransactions = 0
                },
                Transactions = transactionDtos
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("by-day")]
    public async Task<ActionResult<ReportResponse>> GetReportByDay([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (page <= 0 || pageSize <= 0)
            {
                return BadRequest(new { error = "Page and page size must be greater than 0" });
            }

            if (endDate < startDate)
            {
                return BadRequest(new { error = "End date must be greater than or equal to start date" });
            }
            var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate, page, pageSize);
            
            // Get all transactions in the date range to calculate correct totals
            var allTransactionsInRange = await _transactionRepository.GetByDateRangeAsync(startDate, endDate, 1, int.MaxValue);
            var totalCount = allTransactionsInRange.Count();
            var totalAmount = allTransactionsInRange.Where(t => t.IsValid).Sum(t => t.Amount);
            var validCount = allTransactionsInRange.Count(t => t.IsValid);
            var rejectedCount = allTransactionsInRange.Count(t => !t.IsValid);

            var transactionDtos = transactions.Select(MapToDto).ToList();

            var response = new ReportResponse
            {
                Summary = new ReportSummary
                {
                    TotalCount = totalCount,
                    TotalAmount = totalAmount,
                    ValidTransactions = validCount,
                    RejectedTransactions = rejectedCount
                },
                Transactions = transactionDtos
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("rejected")]
    public async Task<ActionResult<ReportResponse>> GetRejectedReport([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
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

            var response = new ReportResponse
            {
                Summary = new ReportSummary
                {
                    TotalCount = totalCount,
                    TotalAmount = 0, // Rejected transactions don't contribute to total amount
                    ValidTransactions = 0,
                    RejectedTransactions = totalCount
                },
                Transactions = transactionDtos
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }


    
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardStatsResponse>> GetDashboardStats([FromQuery] string dateRange = "7d")
    {
        try
        {
            // Parse date range
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now;
            
            // Check if it's a custom date range (format: "startDate|endDate")
            if (dateRange.Contains("|"))
            {
                var dateParts = dateRange.Split('|');
                if (dateParts.Length == 2 && 
                    DateTime.TryParse(dateParts[0], out var customStartDate) && 
                    DateTime.TryParse(dateParts[1], out var customEndDate))
                {
                    startDate = customStartDate;
                    endDate = customEndDate;
                }
                else
                {
                    return BadRequest(new { error = "Invalid custom date range format. Use 'YYYY-MM-DD|YYYY-MM-DD'" });
                }
            }
            else
            {
                // Handle predefined ranges
                switch (dateRange)
                {
                    case "7d":
                        startDate = DateTime.Now.AddDays(-7);
                        break;
                    case "30d":
                        startDate = DateTime.Now.AddDays(-30);
                        break;
                    case "90d":
                        startDate = DateTime.Now.AddDays(-90);
                        break;
                    default:
                        return BadRequest(new { error = "Invalid date range. Use 7d, 30d, 90d, or custom format 'YYYY-MM-DD|YYYY-MM-DD'" });
                }
            }
            
            // Get transactions within the date range
            var transactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate, 1, int.MaxValue);
            
            // Calculate statistics
            int totalTransactions = transactions.Count();
            int validTransactions = transactions.Count(t => t.IsValid);
            int invalidTransactions = totalTransactions - validTransactions;
            decimal totalAmount = transactions.Sum(t => t.IsValid ? t.Amount : 0);
            decimal averageAmount = validTransactions > 0 ? totalAmount / validTransactions : 0;
            
            // Calculate card type distribution
            var cardTypeDistribution = transactions
                .GroupBy(t => t.CardType)
                .Select(g => new DTOs.CardTypeDistribution
                {
                    CardType = g.Key.GetDisplayName(),
                    Count = g.Count(),
                    Percentage = totalTransactions > 0 ? (double)g.Count() / totalTransactions * 100 : 0
                })
                .ToList();
            
            // Get recent transactions (last 10)
            var recentTransactions = transactions
                .OrderByDescending(t => t.Timestamp)
                .Take(10)
                .Select(MapToDto)
                .ToList();
            
            var response = new DTOs.DashboardStatsResponse
            {
                TotalTransactions = totalTransactions,
                ValidTransactions = validTransactions,
                InvalidTransactions = invalidTransactions,
                TotalAmount = totalAmount,
                AverageAmount = averageAmount,
                CardTypeDistribution = cardTypeDistribution,
                RecentTransactions = recentTransactions
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


