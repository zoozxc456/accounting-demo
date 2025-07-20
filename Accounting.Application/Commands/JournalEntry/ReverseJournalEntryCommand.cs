namespace Accounting.Application.Commands.JournalEntry;

public record ReverseJournalEntryCommand(Guid OriginalJournalEntryId, DateTime ReversalDate);