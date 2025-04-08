using System.Text.Json.Serialization;

namespace CloneDevOpsTemplate.Models;

public class GitPullRequests
{
    public int Count { get; set; }
    public GitPullRequest[] Value { get; set; } = [];
}

public class GitPullRequest
{
    public Repository Repository { get; set; } = new();
    public int PullRequestId { get; set; }
    public int CodeReviewId { get; set; }
    public PullRequestStatus Status { get; set; }
    public User CreatedBy { get; set; } = new();
    public DateTime CreationDate { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SourceRefName { get; set; } = string.Empty;
    public string TargetRefName { get; set; } = string.Empty;
    public PullRequestAsyncStatus MergeStatus { get; set; }
    public bool IsDraft { get; set; }
    public Guid MergeId { get; set; }
    public GitCommitRef LastMergeSourceCommit { get; set; } = new();
    public GitCommitRef LastMergeTargetCommit { get; set; } = new();
    public GitCommitRef LastMergeCommit { get; set; } = new();
    public User[] Reviewers { get; set; } = [];
    public bool SupportsIterations { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter<PullRequestStatus>))]
public enum PullRequestStatus
{
    Abandoned,
    Active,
    All,
    Completed,
    NotSet
}

[JsonConverter(typeof(JsonStringEnumConverter<PullRequestAsyncStatus>))]
public enum PullRequestAsyncStatus
{
    Conflicts,
    Failure,
    NotSet,
    Queued,
    RejectedByPolicy,
    Succeeded
}

public class GitCommitRef
{
    public string CommitId { get; set; } = string.Empty;
}