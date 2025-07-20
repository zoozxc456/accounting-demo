using Accounting.Domain.Exceptions;
using Accounting.Domain.ValueObjects;

namespace Accounting.Domain.Aggregates.JournalEntry;

public class JournalEntry
{
    public Guid Id { get; private set; }
    public DateTime Date { get; private set; }
    public string Description { get; private set; }
    public Voucher Voucher { get; private set; }
    public IReadOnlyList<TransactionLine> Lines => _lines.AsReadOnly();
    private readonly List<TransactionLine> _lines = new();

    private JournalEntry(DateTime date, string description, Voucher voucher, List<TransactionLine> lines)
    {
        Id = Guid.NewGuid();
        Date = date;
        Description = description;
        Voucher = voucher;
        _lines = lines;
    }

    public static JournalEntry Create(DateTime date, string description, Voucher voucher, List<TransactionLine> lines)
    {
        if (date == default)
            throw new ArgumentException("日期不可為預設值。", nameof(date));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("描述不可為空。", nameof(description));
        if (lines == null || lines.Count < 2)
            throw new DomainException("分錄至少需要一借一貸。");
        if (voucher is null)
            throw new ArgumentNullException(nameof(voucher), "單據不可為 null。");

        var entry = new JournalEntry(date, description, voucher, lines);
        if (!entry.IsBalanced())
            throw new DomainException("借貸金額不平衡。");

        return entry;
    }
    
    public static JournalEntry Reverse(JournalEntry originalEntry, DateTime reversalDate)
    {
        var reversedLines = originalEntry.Lines.Select(line =>
        {
            // Swap debit and credit
            var amount = line.Debit.Amount > 0 ? line.Debit : line.Credit;
            var isDebit = line.Debit.Amount == 0; // if original was credit (debit=0), new is debit
            return TransactionLine.Create(line.AccountId, amount, isDebit);
        }).ToList();

        var reversedDescription = $"Reversal of: {originalEntry.Description}";
        var reversalVoucherNumber = $"REV-{originalEntry.Voucher.VoucherNumber}";
        var reversalVoucher = new Voucher(reversalVoucherNumber);

        // The Create method will validate the new entry
        return Create(reversalDate, reversedDescription, reversalVoucher, reversedLines);
    }

    public decimal GetTotalDebit() => Lines.Sum(l => l.Debit.Amount);
    public decimal GetTotalCredit() => Lines.Sum(l => l.Credit.Amount);
    public bool IsBalanced() => GetTotalDebit() == GetTotalCredit();
}