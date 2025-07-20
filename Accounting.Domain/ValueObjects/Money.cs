namespace Accounting.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(string currency, decimal amount)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("幣別格式不正確。", nameof(currency));
        if (amount < 0)
            throw new ArgumentException("金額不可為負數。", nameof(amount));

        Currency = currency;
        Amount = amount;
    }

    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("不同幣別的金額無法進行運算。");
        return new Money(a.Currency, a.Amount + b.Amount);
    }
}