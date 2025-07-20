namespace Accounting.Application.DTOs;

public class EntryLineDto
{
    public EntryLineDto()
    {
    }

    public EntryLineDto(Guid accountId, decimal amount, bool isDebit)
    {
        AccountId = accountId;
        Amount = amount;
        IsDebit = isDebit;
    }

    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public bool IsDebit { get; set; }
}