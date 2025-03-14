namespace CloneDevOpsTemplate.Models;

public class CreateProject
{
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public Capabilities capabilities { get; set; } = new Capabilities();
    public string visibility { get; set; } = string.Empty;
}

public class Capabilities
{
    public VersionControl versioncontrol { get; set; } = new VersionControl();
    public ProcessTemplate processTemplate { get; set; } = new ProcessTemplate();
}

public class VersionControl
{
    public string sourceControlType { get; set; } = string.Empty;
}

public class ProcessTemplate
{
    public string templateTypeId { get; set; } = string.Empty;
}

public class CreateProjectResponse : ErrorResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}