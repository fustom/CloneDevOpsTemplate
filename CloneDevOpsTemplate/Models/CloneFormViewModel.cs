namespace CloneDevOpsTemplate.Models;

public record CloneFormViewModel<TItem>(TItem[] Items, string Controller, string Action, MigrationProgress? FinishedMigration = null);
