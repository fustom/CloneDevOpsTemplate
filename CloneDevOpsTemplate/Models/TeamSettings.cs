using System.Text.Json.Serialization;

namespace CloneDevOpsTemplate.Models;

public class TeamSettings
{
    public TeamIteration BacklogIteration { set; get; } = new();
    public string BugsBehavior { set; get; } = string.Empty;
    public string[] WorkingDays { set; get; } = [];
    public BacklogVisibilities BacklogVisibilities { set; get; } = new();
    public TeamIteration DefaultIteration { set; get; } = new();
    public string DefaultIterationMacro { set; get; } = string.Empty;
}

public class BacklogVisibilities
{
    [JsonPropertyName("Microsoft.EpicCategory")]
    public bool EpicCategory { set; get; }
    [JsonPropertyName("Microsoft.FeatureCategory")]
    public bool FeatureCategory { set; get; }
    [JsonPropertyName("Microsoft.RequirementCategory")]
    public bool RequirementCategory { set; get; }
}

public class TeamIteration
{
    public Guid Id { set; get; }
}