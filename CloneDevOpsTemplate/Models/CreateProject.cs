namespace CloneDevOpsTemplate.Models;

public class CreateProject
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Capabilities Capabilities { get; set; } = new();
    public Visibility Visibility { get; set; }
}

public class CreateProjectResponse : ErrorResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}