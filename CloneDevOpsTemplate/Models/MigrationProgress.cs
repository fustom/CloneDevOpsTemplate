namespace CloneDevOpsTemplate.Models;

public class MigrationProgress
{
    public int Step { get; set; }
    public int TotalSteps { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? CurrentDetail { get; set; }
    public List<MigrationProgressDetail> Details { get; set; } = [];
}

public class MigrationProgressDetail
{
    public DateTimeOffset Timestamp { get; set; }
    public int Step { get; set; }
    public string Message { get; set; } = string.Empty;
}
