namespace CloneDevOpsTemplate.Models;

public class Processes
{
    public int Count { get; set; }
    public Process[] Value { get; set; } = [];
}

public class Process
{
    public string TypeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ReferenceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ParentProcessTypeId { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public bool IsDefault { get; set; }
    public string CustomizationType { get; set; } = string.Empty;
}