using Accounting.Application.Commands.JournalEntry;
using Accounting.Application.Handlers.JournalEntry;
using Accounting.Domain.Aggregates.Account;
using Accounting.Domain.Aggregates.JournalEntry;
using Accounting.Domain.Exceptions;
using Accounting.Domain.Repositories;
using Accounting.Domain.ValueObjects;
using JournalEntryAggregate =  Accounting.Domain.Aggregates.JournalEntry.JournalEntry;
using AccountAggregate = Accounting.Domain.Aggregates.Account.Account;
using NSubstitute;

namespace Accounting.UnitTest.Application.JournalEntry;

[TestFixture]
public class ReverseJournalEntryCommandHandlerTests
{
    private IUnitOfWork _unitOfWork;
    private IJournalEntryRepository _journalEntryRepository;
    private IAccountRepository _accountRepository;
    private ReverseJournalEntryCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        _accountRepository = Substitute.For<IAccountRepository>();
        _handler = new ReverseJournalEntryCommandHandler(
            _unitOfWork,
            _journalEntryRepository,
            _accountRepository);
    }

    [Test]
    public async Task Handle_ShouldCreateReversalAndSaveChanges_WhenOriginalEntryExists()
    {
        // Arrange
        var cashAccountId = Guid.NewGuid();
        var expenseAccountId = Guid.NewGuid();
        var originalLines = new List<TransactionLine>
        {
            TransactionLine.Create(cashAccountId, new Money("TWD", 500), isDebit: false), // Credit Cash
            TransactionLine.Create(expenseAccountId, new Money("TWD", 500), isDebit: true) // Debit Expense
        };
        var originalVoucher = new Voucher("V-001");
        var originalEntry = JournalEntryAggregate.Create(new DateTime(2025, 7, 19), "原始交易", originalVoucher, originalLines);

        var command = new ReverseJournalEntryCommand(
            OriginalJournalEntryId: originalEntry.Id,
            ReversalDate: new DateTime(2025, 8, 1));


        _journalEntryRepository.GetByIdAsync(originalEntry.Id)!.Returns(Task.FromResult(originalEntry));

        var cashAccount = AccountAggregate.CreateWithId(cashAccountId, "庫存現金", AccountType.Asset, 1000);
        var expenseAccount = AccountAggregate.CreateWithId(expenseAccountId, "辦公用品", AccountType.Expense, 500);
        _accountRepository.GetByIdAsync(cashAccountId)!.Returns(Task.FromResult(cashAccount));
        _accountRepository.GetByIdAsync(expenseAccountId)!.Returns(Task.FromResult(expenseAccount));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // 驗證新的回沖分錄被儲存
        await _journalEntryRepository.Received(1)
            .AddAsync(Arg.Is<JournalEntryAggregate>(je => je.Description.Contains("Reversal")));
        // 驗證兩個科目都被更新
        await _accountRepository.Received(2).UpdateAsync(Arg.Any<AccountAggregate>());
        // 驗證工作單元被儲存
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Test]
    public void Handle_ShouldThrowNotFoundException_WhenOriginalEntryDoesNotExist()
    {
        // Arrange
        var command = new ReverseJournalEntryCommand(
            OriginalJournalEntryId: Guid.NewGuid(),
            ReversalDate: new DateTime(2025, 8, 1));

        _journalEntryRepository.GetByIdAsync(command.OriginalJournalEntryId)
            !.Returns(Task.FromResult<JournalEntryAggregate>(null));

        // Act & Assert
        Assert.ThrowsAsync<NotFoundException>(async () => await _handler.Handle(command, CancellationToken.None));
    }
}