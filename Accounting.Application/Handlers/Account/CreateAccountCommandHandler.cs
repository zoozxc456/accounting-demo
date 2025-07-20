using Accounting.Application.Commands.Account;
using Accounting.Domain.Repositories;
using AccountAggregate = Accounting.Domain.Aggregates.Account;

namespace Accounting.Application.Handlers.Account;

public class CreateAccountCommandHandler(IUnitOfWork unitOfWork, IAccountRepository accountRepository)
{
    public async Task<Guid> Handle(CreateAccountCommand command, CancellationToken cancellationToken)
    {
        if (await accountRepository.AnyAsync(x => x.Name == command.Name))
            throw new InvalidOperationException();

        var account = AccountAggregate.Account.Create(command.Name, command.Type, command.Balance);
        await accountRepository.AddAsync(account);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return account.Id;
    }
}