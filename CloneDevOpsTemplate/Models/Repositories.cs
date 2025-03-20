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
    public string DefaultBranch {get;set;}  = string.Empty;
    public long Size { get; set; }
    public string RemoteUrl { get; set; } = string.Empty;
    public string SshUrl { get; set; } = string.Empty;
    public string WebUrl { get; set; } = string.Empty;
    public bool IsDisabled { get; set; }
    public bool IsInMaintenance { get; set; }
}