using System.Text.Json.Serialization;

namespace CloneDevOpsTemplate.Models;

public class Projects
{
    public int Count { get; set; }
    public ProjectBase[] Value { get; set; } = [];
}

public class ProjectBase
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public ProjectState State { get; set; }
    public int Revision { get; set; }
    public Visibility Visibility { get; set; }
    public string LastUpdateTime { get; set; } = string.Empty;
}

public class Project : ProjectBase
{
    public BaseTeam DefaultTeam { get; set; } = new();
    public Capabilities Capabilities { get; set; } = new();
}

public class Capabilities
{
    public VersionControl Versioncontrol { get; set; } = new();
    public ProcessTemplate ProcessTemplate { get; set; } = new();
}

public class VersionControl
{
    public string SourceControlType { get; set; } = string.Empty;
    public string? GitEnabled { get; set; }
    public string? TfvcEnabled { get; set; }
}

public class ProcessTemplate
{
    public string? TemplateName { get; set; }
    public Guid TemplateTypeId { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<Visibility>))]
public enum Visibility
{
    Private,
    Public
}

[JsonConverter(typeof(JsonStringEnumConverter<ProjectState>))]
public enum ProjectState
{
    All,
    CreatePending,
    Deleted,
    Deleting,
    New,
    Unchanged,
    WellFormed
}

public class CloneProjectResult
{
    public Project Project { get; set; } = new();
    public Project TemplateProject { get; set; } = new();
    public string? ErrorMessage { get; set; }
}