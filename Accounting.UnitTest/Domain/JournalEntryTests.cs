using Accounting.Domain.Aggregates.JournalEntry;
using Accounting.Domain.Exceptions;
using Accounting.Domain.ValueObjects;

namespace Accounting.UnitTest.Domain;

[TestFixture]
public class JournalEntryTests
{
    [Test]
    public void Create_WhenJournalIsBalanced_ShouldSucceed()
    {
        var lines = new List<TransactionLine>
        {
            TransactionLine.Create(Guid.NewGuid(), new Money("TWD", 100), isDebit: true),
            TransactionLine.Create(Guid.NewGuid(), new Money("TWD", 100), isDebit: false)
        };

        var voucher = new Voucher("1234567890");

        // Act
        var entry = JournalEntry.Create(new DateTime(2025, 7, 19), "測試交易", voucher, lines);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(entry, Is.Not.Null);
            Assert.That(entry.Date, Is.EqualTo(new DateTime(2025, 7, 19)));
            Assert.That(entry.Lines, Is.Not.Null);
            Assert.That(entry.Lines.Count, Is.EqualTo(2));
            Assert.That(entry.IsBalanced(), Is.True);
            Assert.That(entry.Description, Is.EqualTo("測試交易"));
        });
    }

    [Test]
    public void Create_WhenJournalIsNotBalanced_ShouldThrowDomainException()
    {
        var lines = new List<TransactionLine>
        {
            TransactionLine.Create(Guid.NewGuid(), new Money("TWD", 100), isDebit: true),
            TransactionLine.Create(Guid.NewGuid(), new Money("TWD", 99), isDebit: false)
        };
        var voucher = new Voucher("1234567890");

        Assert.Throws<DomainException>(() =>
            JournalEntry.Create(new DateTime(2025, 7, 19), "不平衡交易", voucher, lines), "借貸金額不平衡。");
    }

    [Test]
    public void Create_WhenLineItemsAreLessThanTwo_ShouldThrowDomainException()
    {
        var lines = new List<TransactionLine>
        {
            TransactionLine.Create(Guid.NewGuid(), new Money("TWD", 100), isDebit: true)
        };
        var voucher = new Voucher("1234567890");

        Assert.Throws<DomainException>(() =>
            JournalEntry.Create(new DateTime(2025, 7, 19), "單邊交易", voucher, lines), "分錄至少需要一借一貸。");
    }

    [Test]
    public void Reverse_ShouldCreateOppositeEntry_WithCorrectDetails()
    {
        // Arrange
        var cashAccountId = Guid.NewGuid();
        var expenseAccountId = Guid.NewGuid();
        var originalLines = new List<TransactionLine>
        {
            TransactionLine.Create(cashAccountId, new Money("TWD", 500), isDebit: false), // Credit Cash
            TransactionLine.Create(expenseAccountId, new Money("TWD", 500), isDebit: true) // Debit Expense
        };
        var originalVoucher = new Voucher("EXP-07-123");
        var originalEntry = JournalEntry.Create(new DateTime(2025, 7, 19), "原始交易", originalVoucher, originalLines);
        var reversalDate = new DateTime(2025, 8, 1);

        // Act
        var reversalEntry = JournalEntry.Reverse(originalEntry, reversalDate);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(reversalEntry, Is.Not.Null);
            Assert.That(reversalEntry.Date, Is.EqualTo(reversalDate));
            Assert.That(reversalEntry.Description,Is.EqualTo("Reversal of: 原始交易"));
            Assert.That(reversalEntry.Voucher.VoucherNumber, Is.EqualTo("REV-EXP-07-123"));
            Assert.That(reversalEntry.IsBalanced(), Is.True);
            
            var cashLine = reversalEntry.Lines.Single(l => l.AccountId == cashAccountId);
            var expenseLine = reversalEntry.Lines.Single(l => l.AccountId == expenseAccountId);
            
            Assert.That(cashLine.Debit.Amount, Is.EqualTo(500m));
            Assert.That(cashLine.Credit.Amount, Is.EqualTo(0));
            Assert.That(expenseLine.Debit.Amount, Is.EqualTo(0));
            Assert.That(expenseLine.Credit.Amount, Is.EqualTo(500m));
        });
    }
}