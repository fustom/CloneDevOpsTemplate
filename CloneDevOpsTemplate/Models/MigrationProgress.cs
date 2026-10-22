namespace CloneDevOpsTemplate.Models;

public class MigrationProgress
{
    public int Step { get; set; }
    public int TotalSteps { get; set; }
    public string Message { get; set; } = string.Empty;
}
