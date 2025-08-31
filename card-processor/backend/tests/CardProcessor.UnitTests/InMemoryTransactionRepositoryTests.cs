using CardProcessor.Core;
using CardProcessor.Infrastructure.Repositories;

namespace CardProcessor.UnitTests;

public class InMemoryTransactionRepositoryTests
{
    private readonly InMemoryTransactionRepository _repository;

    public InMemoryTransactionRepositoryTests()
    {
        _repository = new InMemoryTransactionRepository();
    }

    [Fact]
    public async Task AddAsync_WithValidTransaction_ReturnsTransactionWithId()
    {
        // Arrange
        var transaction = new Transaction("4532015112830366", 100.50m, DateTime.UtcNow);

        // Act
        var result = await _repository.AddAsync(transaction);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.CardNumber.Should().Be("4532015112830366");
        result.Amount.Should().Be(100.50m);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingTransaction_ReturnsTransaction()
    {
        // Arrange
        var transaction = new Transaction("4532015112830366", 100.50m, DateTime.UtcNow);
        var addedTransaction = await _repository.AddAsync(transaction);

        // Act
        var result = await _repository.GetByIdAsync(addedTransaction.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(addedTransaction.Id);
        result.CardNumber.Should().Be("4532015112830366");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingTransaction_ReturnsNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleTransactions_ReturnsAllTransactions()
    {
        // Arrange
        var transaction1 = new Transaction("4532015112830366", 100.50m, DateTime.UtcNow);
        var transaction2 = new Transaction("5555555555554444", 200.75m, DateTime.UtcNow);
        
        await _repository.AddAsync(transaction1);
        await _repository.AddAsync(transaction2);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.CardNumber == "4532015112830366");
        result.Should().Contain(t => t.CardNumber == "5555555555554444");
    }

    [Fact]
    public async Task GetAllAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        for (int i = 0; i < 25; i++)
        {
            var transaction = new Transaction("4532015112830366", 100.50m, DateTime.UtcNow);
            await _repository.AddAsync(transaction);
        }

        // Act
        var result = await _repository.GetAllAsync(page: 2, pageSize: 10);

        // Assert
        result.Should().HaveCount(10);
    }

    [Fact]
    public async Task GetByCardTypeAsync_WithValidCardType_ReturnsFilteredTransactions()
    {
        // Arrange
        var visaTransaction = new Transaction("4532015112830366", 100.50m, DateTime.UtcNow);
        var masterCardTransaction = new Transaction("5555555555554444", 200.75m, DateTime.UtcNow);
        
        await _repository.AddAsync(visaTransaction);
        await _repository.AddAsync(masterCardTransaction);

        // Act
        var result = await _repository.GetByCardTypeAsync(CardType.Visa);

        // Assert
        result.Should().HaveCount(1);
        result.First().CardType.Should().Be(CardType.Visa);
    }

    [Fact]
    public async Task GetByDateRangeAsync_WithValidRange_ReturnsFilteredTransactions()
    {
        // Arrange
        var oldTransaction = new Transaction("4532015112830366", 100.50m, DateTime.UtcNow.AddDays(-10));
        var recentTransaction = new Transaction("5555555555554444", 200.75m, DateTime.UtcNow);
        
        await _repository.AddAsync(oldTransaction);
        await _repository.AddAsync(recentTransaction);

        var startDate = DateTime.UtcNow.AddDays(-5);
        var endDate = DateTime.UtcNow.AddDays(1);

        // Act
        var result = await _repository.GetByDateRangeAsync(startDate, endDate);

        // Assert
        result.Should().HaveCount(1);
        result.First().CardNumber.Should().Be("5555555555554444");
    }

    [Fact]
    public async Task GetRejectedTransactionsAsync_WithMixedTransactions_ReturnsOnlyRejected()
    {
        // Arrange
        var validTransaction = new Transaction("4532015112830366", 100.50m, DateTime.UtcNow);
        var invalidTransaction = new Transaction("1234567890123456", 200.75m, DateTime.UtcNow); // Unknown card type
        
        await _repository.AddAsync(validTransaction);
        await _repository.AddAsync(invalidTransaction);

        // Act
        var result = await _repository.GetRejectedTransactionsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task AddRangeAsync_WithMultipleTransactions_AddsAllTransactions()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new Transaction("4532015112830366", 100.50m, DateTime.UtcNow),
            new Transaction("5555555555554444", 200.75m, DateTime.UtcNow),
            new Transaction("378282246310005", 300.25m, DateTime.UtcNow)
        };

        // Act
        var result = await _repository.AddRangeAsync(transactions);

        // Assert
        result.Should().HaveCount(3);
        var allTransactions = await _repository.GetAllAsync();
        allTransactions.Should().HaveCount(3);
    }



    [Fact]
    public async Task GetTotalCountAsync_WithTransactions_ReturnsCorrectCount()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            var transaction = new Transaction("4532015112830366", 100.50m, DateTime.UtcNow);
            await _repository.AddAsync(transaction);
        }

        // Act
        var result = await _repository.GetTotalCountAsync();

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public async Task GetCountByCardTypeAsync_WithMixedTransactions_ReturnsCorrectCount()
    {
        // Arrange
        var visaTransactions = new List<Transaction>
        {
            new Transaction("4532015112830366", 100.50m, DateTime.UtcNow),
            new Transaction("4111111111111111", 200.75m, DateTime.UtcNow)
        };
        
        var masterCardTransactions = new List<Transaction>
        {
            new Transaction("5555555555554444", 300.25m, DateTime.UtcNow)
        };

        await _repository.AddRangeAsync(visaTransactions);
        await _repository.AddRangeAsync(masterCardTransactions);

        // Act
        var visaCount = await _repository.GetCountByCardTypeAsync(CardType.Visa);
        var masterCardCount = await _repository.GetCountByCardTypeAsync(CardType.MasterCard);

        // Assert
        visaCount.Should().Be(2);
        masterCardCount.Should().Be(1);
    }

    [Fact]
    public async Task GetRejectedCountAsync_WithMixedTransactions_ReturnsCorrectCount()
    {
        // Arrange
        var validTransactions = new List<Transaction>
        {
            new Transaction("4532015112830366", 100.50m, DateTime.UtcNow),
            new Transaction("5555555555554444", 200.75m, DateTime.UtcNow)
        };
        
        var invalidTransactions = new List<Transaction>
        {
            // Only use card types that will be detected as invalid without Luhn check
            new Transaction("1234567890123456", 400.00m, DateTime.UtcNow)  // Unknown type
        };

        await _repository.AddRangeAsync(validTransactions);
        await _repository.AddRangeAsync(invalidTransactions);

        // Act
        var result = await _repository.GetRejectedCountAsync();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task GetTotalAmountAsync_WithTransactions_ReturnsCorrectAmount()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new Transaction("4532015112830366", 100.50m, DateTime.UtcNow),
            new Transaction("5555555555554444", 200.75m, DateTime.UtcNow),
            new Transaction("378282246310005", 300.25m, DateTime.UtcNow)
        };

        await _repository.AddRangeAsync(transactions);

        // Act
        var result = await _repository.GetTotalAmountAsync();

        // Assert
        result.Should().Be(601.50m);
    }

    [Fact]
    public async Task GetTotalAmountByCardTypeAsync_WithMixedTransactions_ReturnsCorrectAmount()
    {
        // Arrange
        var visaTransactions = new List<Transaction>
        {
            new Transaction("4532015112830366", 100.50m, DateTime.UtcNow),
            new Transaction("4111111111111111", 200.75m, DateTime.UtcNow)
        };
        
        var masterCardTransactions = new List<Transaction>
        {
            new Transaction("5555555555554444", 300.25m, DateTime.UtcNow)
        };

        await _repository.AddRangeAsync(visaTransactions);
        await _repository.AddRangeAsync(masterCardTransactions);

        // Act
        var visaAmount = await _repository.GetTotalAmountByCardTypeAsync(CardType.Visa);
        var masterCardAmount = await _repository.GetTotalAmountByCardTypeAsync(CardType.MasterCard);

        // Assert
        visaAmount.Should().Be(301.25m);
        masterCardAmount.Should().Be(300.25m);
    }
}


