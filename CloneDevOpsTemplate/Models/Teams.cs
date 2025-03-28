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
    public string ProjectId { get; set; } = string.Empty;
}

public class MapTeams
{
    public Dictionary<Guid, Guid> Teams { get; set; } = [];
}

public class TeamMember
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string UniqueName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Descriptor { get; set; } = string.Empty;
}
