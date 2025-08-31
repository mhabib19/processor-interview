using CardProcessor.Core;

namespace CardProcessor.Infrastructure.Repositories;

public class InMemoryTransactionRepository : ITransactionRepository
{
    private readonly List<Transaction> _transactions = new();
    private readonly object _lock = new();

    public Task<Transaction?> GetByIdAsync(Guid id)
    {
        lock (_lock)
        {
            var transaction = _transactions.FirstOrDefault(t => t.Id == id);
            return Task.FromResult(transaction);
        }
    }

    public Task<IEnumerable<Transaction>> GetAllAsync(int page = 1, int pageSize = 20)
    {
        lock (_lock)
        {
            var skip = (page - 1) * pageSize;
            var transactions = _transactions
                .OrderByDescending(t => t.Timestamp)
                .Skip(skip)
                .Take(pageSize);
            return Task.FromResult(transactions);
        }
    }

    public Task<IEnumerable<Transaction>> GetByCardTypeAsync(CardType cardType, int page = 1, int pageSize = 20)
    {
        lock (_lock)
        {
            var skip = (page - 1) * pageSize;
            var transactions = _transactions
                .Where(t => t.CardType == cardType)
                .OrderByDescending(t => t.Timestamp)
                .Skip(skip)
                .Take(pageSize);
            return Task.FromResult(transactions);
        }
    }

    public Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int page = 1, int pageSize = 20)
    {
        lock (_lock)
        {
            var skip = (page - 1) * pageSize;
            var transactions = _transactions
                .Where(t => t.Timestamp >= startDate && t.Timestamp <= endDate)
                .OrderByDescending(t => t.Timestamp)
                .Skip(skip)
                .Take(pageSize);
            return Task.FromResult(transactions);
        }
    }

    public Task<IEnumerable<Transaction>> GetRejectedTransactionsAsync(int page = 1, int pageSize = 20)
    {
        lock (_lock)
        {
            var skip = (page - 1) * pageSize;
            var transactions = _transactions
                .Where(t => !t.IsValid)
                .OrderByDescending(t => t.Timestamp)
                .Skip(skip)
                .Take(pageSize);
            return Task.FromResult(transactions);
        }
    }

    public Task<Transaction> AddAsync(Transaction transaction)
    {
        lock (_lock)
        {
            transaction.Update();
            _transactions.Add(transaction);
            return Task.FromResult(transaction);
        }
    }

    public Task<IEnumerable<Transaction>> AddRangeAsync(IEnumerable<Transaction> transactions)
    {
        lock (_lock)
        {
            var transactionList = transactions.ToList();
            foreach (var transaction in transactionList)
            {
                transaction.Update();
                _transactions.Add(transaction);
            }
            return Task.FromResult<IEnumerable<Transaction>>(transactionList);
        }
    }

    public Task<int> GetTotalCountAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_transactions.Count);
        }
    }

    public Task<int> GetCountByCardTypeAsync(CardType cardType)
    {
        lock (_lock)
        {
            return Task.FromResult(_transactions.Count(t => t.CardType == cardType));
        }
    }

    public Task<int> GetRejectedCountAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_transactions.Count(t => !t.IsValid));
        }
    }

    public Task<decimal> GetTotalAmountAsync()
    {
        lock (_lock)
        {
            return Task.FromResult(_transactions.Where(t => t.IsValid).Sum(t => t.Amount));
        }
    }

    public Task<decimal> GetTotalAmountByCardTypeAsync(CardType cardType)
    {
        lock (_lock)
        {
            return Task.FromResult(_transactions.Where(t => t.IsValid && t.CardType == cardType).Sum(t => t.Amount));
        }
    }
}


