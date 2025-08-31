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
        transaction.ValidateCard(false); // Explicitly validate

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
        transaction.ValidateCard(false); // Explicitly validate
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
        transaction1.ValidateCard(false); // Explicitly validate
        transaction2.ValidateCard(false); // Explicitly validate
        
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
    public async Task AddRangeAsync_WithMultipleTransactions_AddsAllTransactions()
    {
        // Arrange
        var transaction = new Transaction("4532015112830366", 100.50m, DateTime.UtcNow);
        transaction.ValidateCard(false); // Explicitly validate

        // Act
        await _repository.AddRangeAsync(new[] { transaction });

        // Assert
        var allTransactions = await _repository.GetAllAsync();
        allTransactions.Should().HaveCount(1);
        allTransactions.First().CardNumber.Should().Be("4532015112830366");
    }

    [Fact]
    public async Task GetByCardTypeAsync_WithValidCardType_ReturnsFilteredTransactions()
    {
        // Arrange
        var visaTransaction = new Transaction("4532015112830366", 100.50m, DateTime.UtcNow);
        var masterCardTransaction = new Transaction("5555555555554444", 200.75m, DateTime.UtcNow);
        visaTransaction.ValidateCard(false); // Explicitly validate
        masterCardTransaction.ValidateCard(false); // Explicitly validate
        
        await _repository.AddAsync(visaTransaction);
        await _repository.AddAsync(masterCardTransaction);

        // Act
        var visaTransactions = await _repository.GetByCardTypeAsync(CardType.Visa);
        var masterCardTransactions = await _repository.GetByCardTypeAsync(CardType.MasterCard);

        // Assert
        visaTransactions.Should().HaveCount(1);
        visaTransactions.First().CardType.Should().Be(CardType.Visa);
        masterCardTransactions.Should().HaveCount(1);
        masterCardTransactions.First().CardType.Should().Be(CardType.MasterCard);
    }

    [Fact]
    public async Task GetByDateRangeAsync_WithValidRange_ReturnsFilteredTransactions()
    {
        // Arrange
        var oldTransaction = new Transaction("4532015112830366", 100.50m, DateTime.UtcNow.AddDays(-10));
        var recentTransaction = new Transaction("5555555555554444", 200.75m, DateTime.UtcNow);
        oldTransaction.ValidateCard(false); // Explicitly validate
        recentTransaction.ValidateCard(false); // Explicitly validate
        
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
    public async Task GetRejectedTransactionsAsync_ReturnsOnlyRejectedTransactions()
    {
        // Arrange
        var validTransaction = new Transaction("4532015112830366", 100.50m, DateTime.UtcNow);
        var invalidTransaction = new Transaction("1234567890123456", 200.75m, DateTime.UtcNow); // Unknown card type
        validTransaction.ValidateCard(false); // Explicitly validate
        invalidTransaction.ValidateCard(false); // Explicitly validate
        
        await _repository.AddAsync(validTransaction);
        await _repository.AddAsync(invalidTransaction);

        // Act
        var result = await _repository.GetRejectedTransactionsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().CardNumber.Should().Be("1234567890123456");
        result.First().IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task GetTotalCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var transaction1 = new Transaction("4532015112830366", 100.50m, DateTime.UtcNow);
        var transaction2 = new Transaction("5555555555554444", 200.75m, DateTime.UtcNow);
        transaction1.ValidateCard(false); // Explicitly validate
        transaction2.ValidateCard(false); // Explicitly validate
        
        await _repository.AddAsync(transaction1);
        await _repository.AddAsync(transaction2);

        // Act
        var result = await _repository.GetTotalCountAsync();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetCountByCardTypeAsync_ReturnsCorrectCount()
    {
        // Arrange
        var visaTransaction = new Transaction("4532015112830366", 100.50m, DateTime.UtcNow);
        var masterCardTransaction = new Transaction("5555555555554444", 200.75m, DateTime.UtcNow);
        visaTransaction.ValidateCard(false); // Explicitly validate
        masterCardTransaction.ValidateCard(false); // Explicitly validate
        
        await _repository.AddAsync(visaTransaction);
        await _repository.AddAsync(masterCardTransaction);

        // Act
        var visaCount = await _repository.GetCountByCardTypeAsync(CardType.Visa);
        var masterCardCount = await _repository.GetCountByCardTypeAsync(CardType.MasterCard);

        // Assert
        visaCount.Should().Be(1);
        masterCardCount.Should().Be(1);
    }

    [Fact]
    public async Task GetRejectedCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var validTransaction = new Transaction("4532015112830366", 100.50m, DateTime.UtcNow);
        var invalidTransaction = new Transaction("1234567890123456", 200.75m, DateTime.UtcNow); // Unknown card type
        validTransaction.ValidateCard(false); // Explicitly validate
        invalidTransaction.ValidateCard(false); // Explicitly validate
        
        await _repository.AddAsync(validTransaction);
        await _repository.AddAsync(invalidTransaction);

        // Act
        var result = await _repository.GetRejectedCountAsync();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task GetTotalAmountAsync_ReturnsCorrectAmount()
    {
        // Arrange
        var validTransaction1 = new Transaction("4532015112830366", 100.50m, DateTime.UtcNow);
        var validTransaction2 = new Transaction("5555555555554444", 200.75m, DateTime.UtcNow);
        var invalidTransaction = new Transaction("1234567890123456", 300.25m, DateTime.UtcNow); // Invalid
        validTransaction1.ValidateCard(false); // Explicitly validate
        validTransaction2.ValidateCard(false); // Explicitly validate
        invalidTransaction.ValidateCard(false); // Explicitly validate
        
        await _repository.AddAsync(validTransaction1);
        await _repository.AddAsync(validTransaction2);
        await _repository.AddAsync(invalidTransaction);

        // Act
        var result = await _repository.GetTotalAmountAsync();

        // Assert
        result.Should().Be(301.25m); // Only valid transactions: 100.50 + 200.75
    }

    [Fact]
    public async Task GetTotalAmountByCardTypeAsync_ReturnsCorrectAmount()
    {
        // Arrange
        var visaTransaction = new Transaction("4532015112830366", 100.50m, DateTime.UtcNow);
        var masterCardTransaction = new Transaction("5555555555554444", 200.75m, DateTime.UtcNow);
        visaTransaction.ValidateCard(false); // Explicitly validate
        masterCardTransaction.ValidateCard(false); // Explicitly validate
        
        await _repository.AddAsync(visaTransaction);
        await _repository.AddAsync(masterCardTransaction);

        // Act
        var visaAmount = await _repository.GetTotalAmountByCardTypeAsync(CardType.Visa);
        var masterCardAmount = await _repository.GetTotalAmountByCardTypeAsync(CardType.MasterCard);

        // Assert
        visaAmount.Should().Be(100.50m);
        masterCardAmount.Should().Be(200.75m);
    }
}


