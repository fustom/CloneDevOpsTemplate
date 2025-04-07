using System.Text.Json.Serialization;

namespace CloneDevOpsTemplate.Models;

public class ClassificationNodeBase
{
    public string Name { get; set; } = string.Empty;
    public Attributes Attributes { get; set; } = new();
}

public class Iteration : ClassificationNodeBase
{
    public int Id { get; set; }
    public Guid Identifier { get; set; }
    public TreeNodeStructureType StructureType { get; set; }
    public bool HasChildren { get; set; }
    public string Path { get; set; } = string.Empty;
    public List<Iteration> Children { get; set; } = [];
    public string Url { get; set; } = string.Empty;
}

public class Attributes
{
    public DateTime? StartDate { get; set; }
    public DateTime? FinishDate { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<TreeNodeStructureType>))]
public enum TreeNodeStructureType
{
    Area,
    Iteration
}

public enum TreeStructureGroup
{
    Areas,
    Iterations
}
