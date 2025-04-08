using System.Text.Json.Serialization;

namespace CloneDevOpsTemplate.Models;

public class Repositories
{
    public int Count { set; get; }
    public Repository[] Value { set; get; } = [];
}

public class Repository
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public ProjectBase Project { get; set; } = new();
    public string DefaultBranch { get; set; } = string.Empty;
    public long Size { get; set; }
    public string RemoteUrl { get; set; } = string.Empty;
    public string SshUrl { get; set; } = string.Empty;
    public string WebUrl { get; set; } = string.Empty;
    public bool IsDisabled { get; set; }
    public bool IsInMaintenance { get; set; }
}

public class GitImportRequestBase
{
    public ImportRequestParameters Parameters { get; set; } = new();
}

public class GitImportRequest : GitImportRequestBase
{
    public int ImportRequestId { get; set; }
    public GitAsyncOperationStatus Status { get; set; }
}

public class ImportRequestParameters
{
    public GitSource GitSource { get; set; } = new();
    public Guid ServiceEndpointId { get; set; }
    public bool DeleteServiceEndpointAfterImportIsDone { get; set; }
}

public class GitSource
{
    public string Url { get; set; } = string.Empty;
}

[JsonConverter(typeof(JsonStringEnumConverter<GitAsyncOperationStatus>))]
public enum GitAsyncOperationStatus
{
    Abandoned,
    Completed,
    Failed,
    InProgress,
    Queued
}
