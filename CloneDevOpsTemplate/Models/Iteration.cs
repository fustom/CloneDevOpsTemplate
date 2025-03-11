namespace CloneDevOpsTemplate.Models;

public class Iterations
{
    public int Count { get; set; }
    public Iteration[] Value { get; set; } = [];
}
public class Iteration
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public Attributes IterationAttributes { get; set; } = new();
    public string Url { get; set; } = string.Empty;
}

public class Attributes
{
    public DateTime? StartDate { get; set; }
    public DateTime? FinishDate { get; set; }
    public string TimeFrame { get; set; } = string.Empty;
}
