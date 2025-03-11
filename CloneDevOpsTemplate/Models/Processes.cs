namespace CloneDevOpsTemplate.Models;

class Processes
{
    int Count { get; set; }
    Process[] Value { get; set; } = [];
}

class Process
{
    string TypeId { get; set; } = string.Empty;
    string Name { get; set; } = string.Empty;
    string ReferenceName { get; set; } = string.Empty;
    string Description { get; set; } = string.Empty;
    string ParentProcessTypeId { get; set; } = string.Empty;
    bool IsEnabled { get; set; }
    bool IsDefault { get; set; }
    string CustomizationType { get; set; } = string.Empty;
}