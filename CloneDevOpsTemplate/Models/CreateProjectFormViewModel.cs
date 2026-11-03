namespace CloneDevOpsTemplate.Models;

public record CreateProjectFormViewModel(ProjectBase[] Projects, string Controller, string Action, MigrationProgress? FinishedMigration = null);
