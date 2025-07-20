using Accounting.Application.Commands.JournalEntry;
using Accounting.Domain.Aggregates.JournalEntry;
using Accounting.Domain.Exceptions;
using Accounting.Domain.Interfaces;
using Accounting.Domain.Repositories;
using Accounting.Domain.ValueObjects;
using JournalEntryAggregate = Accounting.Domain.Aggregates.JournalEntry;

namespace Accounting.Application.Handlers.JournalEntry;

public class RecordJournalEntryCommandHandler(
    IUnitOfWork unitOfWork,
    IJournalEntryRepository journalEntryRepository,
    IAccountRepository accountRepository,
    IVoucherNumberGenerator voucherNumberGenerator)
{
    public async Task Handle(RecordJournalEntryCommand command, CancellationToken cancellationToken)
    {
        var lines = new List<TransactionLine>();

        foreach (var entry in command.Entries)
        {
            var account = await accountRepository.GetByIdAsync(entry.AccountId);
            if (account is null) throw new NotFoundException("Account not found");

            var money = new Money("TWD", entry.Amount);
            var line = TransactionLine.Create(account.Id, money, entry.IsDebit);
            if (entry.IsDebit)
            {
                account.Debit(entry.Amount);
            }
            else
            {
                account.Credit(entry.Amount);
            }

            lines.Add(line);
            await accountRepository.UpdateAsync(account);
        }

        var voucherNumber = await voucherNumberGenerator.GenerateAsync(command.Date);
        var voucher = new Voucher(voucherNumber);
        var journalEntry = JournalEntryAggregate.JournalEntry.Create(command.Date, command.Description, voucher, lines);

        await journalEntryRepository.AddAsync(journalEntry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}