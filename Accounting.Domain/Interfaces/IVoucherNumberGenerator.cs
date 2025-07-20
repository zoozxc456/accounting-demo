namespace Accounting.Domain.Interfaces;

public interface IVoucherNumberGenerator
{
    Task<string> GenerateAsync(DateTime entryDate);
}