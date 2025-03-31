using System.Text.Json.Serialization;

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
    public OperationStatus Status { get; set; }
    public string Url { get; set; } = string.Empty;
}

[JsonConverter(typeof(JsonStringEnumConverter<OperationStatus>))]
public enum OperationStatus
{
    Cancelled,
    Failed,
    InProgress,
    NotSet,
    Queued,
    Succeeded
}