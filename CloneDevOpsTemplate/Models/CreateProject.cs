namespace CloneDevOpsTemplate.Models;

public class CreateProject
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Capabilities Capabilities { get; set; } = new Capabilities();
}

public class Capabilities
{
    public VersionControl VersionControl { get; set; } = new VersionControl();
    public ProcessTemplate ProcessTemplate { get; set; } = new ProcessTemplate();
}

public class VersionControl
{
    public string SourceControlType { get; set; } = string.Empty;
}

public class ProcessTemplate
{
    public string TemplateTypeId { get; set; } = string.Empty;
}

public class CreateProjectResponse
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}