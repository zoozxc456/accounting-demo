using Accounting.Application.Commands.Account;
using Accounting.Domain.Exceptions;
using Accounting.Domain.Repositories;

namespace Accounting.Application.Handlers.Account;

public class UpdateAccountNameCommandHandler(IUnitOfWork unitOfWork, IAccountRepository accountRepository)
{
    public async Task Handle(UpdateAccountNameCommand command, CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetByIdAsync(command.AccountId);

        if (account is null) throw new NotFoundException("Account not found");

        account.UpdateName(command.Name);

        await accountRepository.UpdateAsync(account);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}