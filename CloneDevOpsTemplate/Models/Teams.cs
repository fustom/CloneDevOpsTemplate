namespace CloneDevOpsTemplate.Models;

public class Teams
{
    public int Count { get; set; }
    public Team[] Value { get; set; } = [];
}

public class BaseTeam
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class Team : BaseTeam
{
    public string Description { get; set; } = string.Empty;
    public string IdentityUrl { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
}

public class MapTeams
{
    public Dictionary<Guid, Guid> Teams { get; set; } = [];
}
