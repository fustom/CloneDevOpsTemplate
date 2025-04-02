using System.Text.Json.Serialization;

namespace CloneDevOpsTemplate.Models;

public class WorkItemsListQueryRequest
{
    public string Query { get; set; } = string.Empty;
}

public class WorkItemsListQueryResult
{
    public string QueryType { get; set; } = string.Empty;
    public string QueryResultType { get; set; } = string.Empty;
    public DateTime AsOf { get; set; }
    public WorkItemsListQueryColumn[] Columns { get; set; } = [];
    public WorkItemsListQueryItem[] WorkItems { get; set; } = [];
}

public class WorkItemsListQueryColumn
{
    public string ReferenceName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class WorkItemsListQueryItem
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
}

public class WorkItem
{
    public int Id { get; set; }
    public int Rev { get; set; }
    public Fields Fields { get; set; } = new();
    public Relations[] Relations { get; set; } = [];
    public string Url { get; set; } = string.Empty;
}

public class Fields
{
    [JsonPropertyName("System.AreaPath")]
    public string SystemAreaPath { get; set; } = string.Empty;
    [JsonPropertyName("System.TeamProject")]
    public string SystemTeamProject { get; set; } = string.Empty;
    [JsonPropertyName("System.IterationPath")]
    public string SystemIterationPath { get; set; } = string.Empty;
    [JsonPropertyName("System.WorkItemType")]
    public string SystemWorkItemType { get; set; } = string.Empty;
    [JsonPropertyName("System.State")]
    public string SystemState { get; set; } = string.Empty;
    [JsonPropertyName("System.Reason")]
    public string SystemReason { get; set; } = string.Empty;
    [JsonPropertyName("System.CreatedDate")]
    public DateTime SystemCreatedDate { get; set; }
    [JsonPropertyName("System.CreatedBy")]
    public TeamMember SystemCreatedBy { get; set; } = new();
    [JsonPropertyName("System.ChangedDate")]
    public DateTime SystemChangedDate { get; set; }
    [JsonPropertyName("System.ChangedBy")]
    public TeamMember SystemChangedBy { get; set; } = new();
    [JsonPropertyName("System.Title")]
    public string SystemTitle { get; set; } = string.Empty;
    [JsonPropertyName("Microsoft.VSTS.Scheduling.Effort")]
    public float MicrosoftVSTSSchedulingEffort { get; set; }
    [JsonPropertyName("System.Description")]
    public string SystemDescription { get; set; } = string.Empty;
    [JsonPropertyName("System.AssignedTo")]
    public TeamMember SystemAssignedTo { get; set; } = new();
    [JsonPropertyName("System.BoardLane")]
    public string SystemBoardLane { get; set; } = string.Empty;
    [JsonPropertyName("Microsoft.VSTS.Scheduling.RemainingWork")]
    public float MicrosoftVSTSSchedulingRemainingWork { get; set; }
    [JsonPropertyName("Microsoft.VSTS.Common.Priority")]
    public float MicrosoftVSTSCommonPriority { get; set; }
    [JsonPropertyName("System.Tags")]
    public string SystemTags { get; set; } = string.Empty;
    [JsonPropertyName("Microsoft.VSTS.TCM.Steps")]
    public string MicrosoftVSTSTCMSteps { get; set; } = string.Empty;
    [JsonPropertyName("Microsoft.VSTS.TCM.Parameters")]
    public string MicrosoftVSTSTCMParameters { get; set; } = string.Empty;
    [JsonPropertyName("Microsoft.VSTS.TCM.LocalDataSource")]
    public string MicrosoftVSTSTCMLocalDataSource { get; set; } = string.Empty;
    [JsonPropertyName("Microsoft.VSTS.TCM.AutomationStatus")]
    public string MicrosoftVSTSTCMAutomationStatus { get; set; } = string.Empty;
    [JsonPropertyName("Microsoft.VSTS.Common.AcceptanceCriteria")]
    public string MicrosoftVSTSCommonAcceptanceCriteria { get; set; } = string.Empty;
}

public class Relations
{
    public string Rel { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public Dictionary<string, string> Attributes { get; set; } = [];
}

public class WorkItemsQueryRequest
{
    public int[] Ids { get; set; } = [];
}

public class WorkItems
{
    public int Count { get; set; }
    public WorkItem[] Value { get; set; } = [];
}
