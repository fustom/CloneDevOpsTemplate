namespace CloneDevOpsTemplate.Models;

public class CloneTeamsResult : CloneResult
{
    public Team[] TemplateTeams { get; init; } = [];
    public Dictionary<Guid, Guid> TeamMap { get; init; } = [];
}
