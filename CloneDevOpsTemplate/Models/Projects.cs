namespace CloneDevOpsTemplate.Models;

public class Projects
{
    public int Count { get; set; }
    public Project[] Value { get; set; } = [];
}

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int Revision { get; set; }
    public string Visibility { get; set; } = string.Empty;
    public string LastUpdateTime { get; set; } = string.Empty;
}