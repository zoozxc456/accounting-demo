namespace Accounting.Domain.ValueObjects;

public record Voucher
{
    public string VoucherNumber { get; }

    public Voucher(string voucherNumber)
    {
        if (string.IsNullOrWhiteSpace(voucherNumber))
            throw new ArgumentException("單據號碼不可為空。", nameof(voucherNumber));
        VoucherNumber = voucherNumber;
    }
}