using Accounting.Application.DTOs;

namespace Accounting.Application.Commands.JournalEntry;

public record RecordJournalEntryCommand(DateTime Date, string Description, List<EntryLineDto> Entries);