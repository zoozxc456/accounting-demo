namespace Accounting.Application.Commands.Account;

public record UpdateAccountNameCommand(Guid AccountId, string Name);