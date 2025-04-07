using System.Text.Json.Serialization;

namespace CloneDevOpsTemplate.Models;

[JsonConverter(typeof(JsonStringEnumConverter<BugsBehavior>))]
public enum BugsBehavior
{
    AsRequirements,
    AsTasks,
    Off
}

[JsonConverter(typeof(JsonStringEnumConverter<DayOfWeek>))]
public enum DayOfWeek
{
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday,
    Saturday,
    Sunday
}

public class TeamSettingsBase
{
    public BugsBehavior BugsBehavior { set; get; }
    public DayOfWeek[] WorkingDays { set; get; } = [];
    public BacklogVisibilities BacklogVisibilities { set; get; } = new();
    public string? DefaultIterationMacro { set; get; }
}

public class PatchTeamSettings : TeamSettingsBase
{
    public Guid? BacklogIteration { set; get; }
    public Guid? DefaultIteration { set; get; }
}

public class TeamSettings : TeamSettingsBase
{
    public TeamIterationSettings? BacklogIteration { set; get; }
    public TeamIterationSettings? DefaultIteration { set; get; }
}

public class BacklogVisibilities
{
    [JsonPropertyName("Microsoft.EpicCategory")]
    public bool? EpicCategory { set; get; }
    [JsonPropertyName("Microsoft.FeatureCategory")]
    public bool? FeatureCategory { set; get; }
    [JsonPropertyName("Microsoft.RequirementCategory")]
    public bool? RequirementCategory { set; get; }
}

public class TeamIterationSettings
{
    public TeamIterationAttributes Attributes { set; get; } = new();
    public Guid Id { set; get; }
    public string Name { set; get; } = string.Empty;
    public string Path { set; get; } = string.Empty;
    public string Url { set; get; } = string.Empty;
}

public class TeamIterationAttributes : Attributes
{
    public TimeFrame TimeFrame { set; get; }
}

[JsonConverter(typeof(JsonStringEnumConverter<TimeFrame>))]
public enum TimeFrame
{
    Current,
    Future,
    Past
}

public class TeamFieldValues
{
    public string DefaultValue { get; set; } = string.Empty;
    public Values[] Values { get; set; } = [];
}

public class Values
{
    public string Value { get; set; } = string.Empty;
    public bool IncludeChildren { get; set; }
}

public class TeamIterations
{
    public int Count { get; set; }
    public TeamIterationSettings[] Value { get; set; } = [];
}
