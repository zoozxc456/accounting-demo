using Accounting.Application.Commands.JournalEntry;
using Accounting.Domain.Exceptions;
using Accounting.Domain.Repositories;
using JournalEntryAggregate = Accounting.Domain.Aggregates.JournalEntry.JournalEntry;

namespace Accounting.Application.Handlers.JournalEntry;

public class ReverseJournalEntryCommandHandler(
    IUnitOfWork unitOfWork,
    IJournalEntryRepository journalEntryRepository,
    IAccountRepository accountRepository)
{
    public async Task Handle(ReverseJournalEntryCommand command, CancellationToken cancellationToken)
    {
        var originalEntry = await journalEntryRepository.GetByIdAsync(command.OriginalJournalEntryId);
        if (originalEntry is null)
        {
            throw new NotFoundException("Original journal entry not found");
        }

        var reversalEntry = JournalEntryAggregate.Reverse(originalEntry, command.ReversalDate);

        foreach (var line in reversalEntry.Lines)
        {
            var account = await accountRepository.GetByIdAsync(line.AccountId);
            if (account is null) throw new NotFoundException("Account not found");
            // We assume account exists as it was part of the original entry
            if (line.Debit.Amount > 0)
            {
                account.Debit(line.Debit.Amount);
            }
            else
            {
                account.Credit(line.Credit.Amount);
            }

            await accountRepository.UpdateAsync(account);
        }

        await journalEntryRepository.AddAsync(reversalEntry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}