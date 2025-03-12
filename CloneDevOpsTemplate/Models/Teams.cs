namespace CloneDevOpsTemplate.Models;

public class Teams
{
    public int Count { get; set; }
    public Team[] Value { get; set; } = [];
}

public class Team
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IdentityUrl { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
}
