namespace CloneDevOpsTemplate.Models;

public record MigrationProgressPanelViewModel(Guid OperationId, string FormId, string SubmitButtonId, MigrationProgress? FinishedMigration);
