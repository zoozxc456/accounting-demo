using Accounting.Application.Commands.JournalEntry;
using Accounting.Application.DTOs;
using Accounting.Application.Handlers.JournalEntry;
using Accounting.Domain.Aggregates.Account;
using Accounting.Domain.Exceptions;
using Accounting.Domain.Interfaces;
using Accounting.Domain.Repositories;
using JournalEntryAggregate = Accounting.Domain.Aggregates.JournalEntry.JournalEntry;
using AccountAggregate = Accounting.Domain.Aggregates.Account.Account;
using NSubstitute;

namespace Accounting.UnitTest.Application.JournalEntry;

[TestFixture]
public class RecordJournalEntryCommandHandlerTests
{
    private IUnitOfWork _unitOfWork;
    private IJournalEntryRepository _journalEntryRepository;
    private IAccountRepository _accountRepository;
    private IVoucherNumberGenerator _voucherNumberGenerator;
    private RecordJournalEntryCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        _accountRepository = Substitute.For<IAccountRepository>();
        _voucherNumberGenerator = Substitute.For<IVoucherNumberGenerator>();
        _handler = new RecordJournalEntryCommandHandler(
            _unitOfWork,
            _journalEntryRepository,
            _accountRepository,
            _voucherNumberGenerator);
    }

    [Test]
    public async Task Handle_WhenCommandIsValidAndBalanced_ShouldSaveAndUdpateSuccessfully()
    {
        var cashAccountId = Guid.NewGuid();
        var expenseAccountId = Guid.NewGuid();
        var command = new RecordJournalEntryCommand(
            Date: new DateTime(2025, 7, 19),
            Description: "購買辦公用品",
            Entries:
            [
                new EntryLineDto(accountId: cashAccountId, amount: 50, isDebit: false),
                new EntryLineDto(accountId: expenseAccountId, amount: 50, isDebit: true)
            ]);

        // 2. 設定模擬倉儲的行為
        var cashAccount = AccountAggregate.Create("庫存現金", AccountType.Asset, 1000);
        var expenseAccount = AccountAggregate.Create("辦公用品", AccountType.Expense, 0);
        _accountRepository.GetByIdAsync(cashAccountId)!.Returns(Task.FromResult(cashAccount));
        _accountRepository.GetByIdAsync(expenseAccountId)!.Returns(Task.FromResult(expenseAccount));

        _voucherNumberGenerator.GenerateAsync(command.Date).Returns(Task.FromResult("1234567890"));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // 驗證 JournalEntryRepository 的 AddAsync 是否被呼叫了一次
        // 我們使用 Arg.Is<T> 來驗證傳入的物件是否符合特定條件 (例如是平衡的)
        await _journalEntryRepository.Received(1).AddAsync(Arg.Is<JournalEntryAggregate>(je => je.IsBalanced()));

        // 驗證 AccountRepository 的 UpdateAsync 是否被呼叫了兩次 (每個科目一次)
        await _accountRepository.Received(2).UpdateAsync(Arg.Any<AccountAggregate>());

        // 驗證 UnitOfWork 的 SaveChangesAsync 是否被呼叫了一次
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task Handle_WhenAccountIdDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var nonExistentAccountId = Guid.NewGuid();
        var command = new RecordJournalEntryCommand(
            Date: new DateTime(2025, 7, 19),
            Description: "無效交易",
            Entries: [new EntryLineDto { AccountId = nonExistentAccountId, Amount = 50, IsDebit = true }]
        );

        // 設定模擬倉儲: 當用這個 ID 查詢時，回傳 null
        _accountRepository.GetByIdAsync(nonExistentAccountId)!.Returns(Task.FromResult<AccountAggregate>(null));
        _voucherNumberGenerator.GenerateAsync(command.Date).Returns(Task.FromResult("1234567890"));

        // Act & Assert
        Assert.ThrowsAsync<NotFoundException>(async () => await _handler.Handle(command, CancellationToken.None),
            "Account not found");

        // 驗證在發生錯誤時，沒有進行任何儲存動作
        await _journalEntryRepository.DidNotReceive().AddAsync(Arg.Any<JournalEntryAggregate>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }

    [Test]
    public async Task Handle_WhenJournalIsUnbalanced_ShouldThrowDomainException()
    {
        // Arrange
        // 1. 準備一個借貸不平衡的 command
        var cashAccountId = Guid.NewGuid();
        var expenseAccountId = Guid.NewGuid();
        var command = new RecordJournalEntryCommand(
            Date: new DateTime(2025, 7, 19),
            Description: "不平衡的交易",
            Entries:
            [
                new EntryLineDto { AccountId = cashAccountId, Amount = 100, IsDebit = false }, // 貸 100
                new EntryLineDto { AccountId = expenseAccountId, Amount = 90, IsDebit = true }
            ]);

        // 2. 確保倉儲可以找到科目

        var cashAccount = AccountAggregate.Create("庫存現金", AccountType.Asset, 1000);
        var expenseAccount = AccountAggregate.Create("辦公用品", AccountType.Expense, 0);
        _accountRepository.GetByIdAsync(cashAccountId)!.Returns(Task.FromResult(cashAccount));
        _accountRepository.GetByIdAsync(expenseAccountId)!.Returns(Task.FromResult(expenseAccount));
        _voucherNumberGenerator.GenerateAsync(command.Date).Returns(Task.FromResult("1234567890"));

        // Act & Assert
        // 驗證 Handle 方法最終會因為 JournalEntry.Create 的規則而拋出 DomainException
        Assert.ThrowsAsync<DomainException>(async () => await _handler.Handle(command, CancellationToken.None),
            "借貸金額不平衡。");

        // 驗證即使科目被更新了，最終的儲存動作也沒有被執行
        await _journalEntryRepository.DidNotReceive().AddAsync(Arg.Any<JournalEntryAggregate>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync();
    }
}