using Accounting.Application.Commands.Account;
using Accounting.Application.Handlers.Account;
using Accounting.Domain.Aggregates.Account;
using Accounting.Domain.Exceptions;
using Accounting.Domain.Repositories;
using AccountAggregate = Accounting.Domain.Aggregates.Account.Account;
using NSubstitute;

namespace Accounting.UnitTest.Application.Account;

[TestFixture]
public class UpdateAccountNameCommandHandlerTests
{
    private IAccountRepository _accountRepository;
    private IUnitOfWork _unitOfWork;
    private UpdateAccountNameCommandHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _handler = new UpdateAccountNameCommandHandler(_unitOfWork, _accountRepository);
    }

    [Test]
    public async Task Handle_WhenAccountExists_ShouldUpdateNameAndSaveChanges()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var command = new UpdateAccountNameCommand(AccountId: accountId, Name: "應付帳款（新）");

        var existingAccount = AccountAggregate.CreateWithId(accountId, "應付帳款", AccountType.Liability);
        _accountRepository.GetByIdAsync(accountId)!.Returns(Task.FromResult(existingAccount));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _accountRepository.Received(1)
            .UpdateAsync(Arg.Is<AccountAggregate>(a => a.Id == accountId && a.Name == command.Name));
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Test]
    public void Handle_WhenAccountDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var command = new UpdateAccountNameCommand(AccountId: accountId, Name: "應付帳款（新）");
        _accountRepository.GetByIdAsync(accountId)!.Returns(Task.FromResult<AccountAggregate>(null));

        // Act & Assert
        Assert.ThrowsAsync<NotFoundException>(async () => await _handler.Handle(command, CancellationToken.None),
            "Account not found");
    }
}