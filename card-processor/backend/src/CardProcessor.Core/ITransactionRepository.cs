namespace CardProcessor.Core;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<IEnumerable<Transaction>> GetAllAsync(int page = 1, int pageSize = 20);
    Task<IEnumerable<Transaction>> GetByCardTypeAsync(CardType cardType, int page = 1, int pageSize = 20);
    Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 20);
    Task<IEnumerable<Transaction>> GetRejectedTransactionsAsync(int page = 1, int pageSize = 20);
    Task<Transaction> AddAsync(Transaction transaction);
    Task<IEnumerable<Transaction>> AddRangeAsync(IEnumerable<Transaction> transactions);
    Task<int> GetTotalCountAsync();
    Task<int> GetCountByCardTypeAsync(CardType cardType);
    Task<int> GetRejectedCountAsync();
    Task<decimal> GetTotalAmountAsync();
    Task<decimal> GetTotalAmountByCardTypeAsync(CardType cardType);
}


