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
    public string State { get; set; } = string.Empty;
    public int Revision { get; set; }
    public string Visibility { get; set; } = string.Empty;
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
    public string TemplateTypeId { get; set; } = string.Empty;
}
