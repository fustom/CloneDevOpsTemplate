namespace CloneDevOpsTemplate.Models;

public class Iteration : ErrorResponse
{
    public int Id { get; set; }
    public Guid Identifier { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StructureType { get; set; } = string.Empty;
    public bool HasChildren { get; set; }
    public string Path { get; set; } = string.Empty;
    public List<Iteration> Children { get; set; } = [];
    public string Url { get; set; } = string.Empty;
    public Attributes Attributes { get; set; } = new();
}

public class Attributes
{
    public DateTime? StartDate { get; set; }
    public DateTime? FinishDate { get; set; }
}

public class CreateIterationRequest
{
    public string Name { get; set; } = string.Empty;
    public Attributes Attributes { get; set; } = new();
}
