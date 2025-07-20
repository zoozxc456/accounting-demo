using Accounting.Domain.ValueObjects;

namespace Accounting.Domain.Aggregates.JournalEntry;

public class TransactionLine
{
    public Guid AccountId { get; private set; }
    public Money Debit { get; private set; }
    public Money Credit { get; private set; }

    private TransactionLine(Guid accountId, Money amount, bool isDebit)
    {
        AccountId = accountId;
        if (isDebit)
        {
            Debit = amount;
            Credit = new Money(amount.Currency, 0);
        }
        else
        {
            Debit = new Money(amount.Currency, 0);
            Credit = amount;
        }
    }

    public static TransactionLine Create(Guid accountId, Money amount, bool isDebit)
    {
        return new TransactionLine(accountId, amount, isDebit);
    }
}