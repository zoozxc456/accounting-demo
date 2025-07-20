using Accounting.Application.Commands.Account;
using Accounting.Application.Handlers.Account;
using Accounting.Domain.Aggregates.Account;
using Accounting.Domain.Repositories;
using AccountAggregate = Accounting.Domain.Aggregates.Account.Account;
using NSubstitute;

namespace Accounting.UnitTest.Application.Account;

[TestFixture]
public class CreateAccountCommandHandlerTests
{
    private IUnitOfWork _unitOfWork;
    private IAccountRepository _accountRepository;
    private CreateAccountCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _accountRepository = Substitute.For<IAccountRepository>();
        _handler = new CreateAccountCommandHandler(_unitOfWork, _accountRepository);
    }

    [Test]
    public async Task Handle_WhenCommandIsValid_ShouldCreateAndSaveAccount()
    {
        // Arrange
        var command = new CreateAccountCommand(Name: "應收帳款", Type: AccountType.Asset, Balance: 1000);

        // Act
        var accountId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.That(accountId, Is.Not.EqualTo(Guid.Empty));
        await _accountRepository.Received(1).AddAsync(Arg.Is<AccountAggregate>(a => a.Name == command.Name));
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Test]
    public void Handle_WhenBalanceIsNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var command = new CreateAccountCommand(Name: "應收帳款", Type: AccountType.Asset, Balance: -1000);

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => await _handler.Handle(command, CancellationToken.None));
    }
}