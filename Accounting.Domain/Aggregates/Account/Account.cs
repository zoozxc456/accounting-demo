namespace Accounting.Domain.Aggregates.Account;

public class Account
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public AccountType Type { get; private set; }
    public decimal Balance { get; private set; }

    private Account(Guid id, string name, AccountType type, decimal initialBalance = 0)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id 不可為空。", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("科目名稱不可為空。", nameof(name));

        if (initialBalance < 0)
            throw new ArgumentException("初始餘額不可低於0。", nameof(initialBalance));

        Id = id;
        Name = name;
        Type = type;
        Balance = initialBalance;
    }

    public static Account Create(string name, AccountType type, decimal initialBalance = 0)
    {
        return new Account(Guid.NewGuid(), name, type, initialBalance);
    }

    public static Account CreateWithId(Guid id, string name, AccountType type, decimal initialBalance = 0)
    {
        return new Account(id, name, type, initialBalance);
    }

    public void Debit(decimal amount)
    {
        if (Type is AccountType.Asset or AccountType.Expense)
            Balance += amount;
        else
            Balance -= amount;
    }

    public void Credit(decimal amount)
    {
        if (Type is AccountType.Asset or AccountType.Expense)
            Balance -= amount;
        else
            Balance += amount;
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException();

        Name = name.Trim();
    }
}