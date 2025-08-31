using CardProcessor.Core;

namespace CardProcessor.API.DTOs;

public class DashboardStatsResponse
{
    public int TotalTransactions { get; set; }
    public int ValidTransactions { get; set; }
    public int InvalidTransactions { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageAmount { get; set; }
    public List<CardTypeDistribution> CardTypeDistribution { get; set; } = new();
    public List<TransactionDto> RecentTransactions { get; set; } = new();
}

public class CardTypeDistribution
{
    public string CardType { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}
