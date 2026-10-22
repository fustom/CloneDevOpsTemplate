using System.Text.Json.Nodes;

namespace CloneDevOpsTemplate.Models;

public class PolicyConfigurations
{
    public int Count { get; set; }
    public PolicyConfiguration[] Value { get; set; } = [];
}

public class PolicyConfiguration
{
    public int Id { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsBlocking { get; set; }
    public bool IsDeleted { get; set; }
    public PolicyType Type { get; set; } = new();
    public JsonObject Settings { get; set; } = new();
}

public class PolicyType
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
