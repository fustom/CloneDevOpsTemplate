namespace CloneDevOpsTemplate.Models;

public class CreateProject
{
    string Name { get; set; } = string.Empty;
    string Description { get; set; } = string.Empty;
    Capabilities Capabilities { get; set; } = new Capabilities();
}

public class Capabilities
{
    VersionControl VersionControl { get; set; } = new VersionControl();
    ProcessTemplate ProcessTemplate { get; set; } = new ProcessTemplate();
}

public class VersionControl
{
    string SourceControlType { get; set; } = string.Empty;
}

public class ProcessTemplate
{
    string TemplateTypeId { get; set; } = string.Empty;
}