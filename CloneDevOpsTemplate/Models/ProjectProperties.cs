namespace CloneDevOpsTemplate.Models;

public class ProjectProperties
{
    public int Count { get; set; }
    public ProjectProperty[] Value { get; set; } = [];
}

public class ProjectProperty
{
    public string Name { get; set; } = string.Empty;
    public object Value { get; set; } = null!;
}