using Accounting.Domain.Aggregates.Account;

namespace Accounting.Application.Commands.Account;

public record CreateAccountCommand(string Name, AccountType Type, decimal Balance);