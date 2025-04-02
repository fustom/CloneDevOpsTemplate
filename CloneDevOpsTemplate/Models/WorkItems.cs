using System.Text.Json.Serialization;

namespace CloneDevOpsTemplate.Models;

public class WorkItemsListQueryRequest
{
    public string Query { get; set; } = "";
}

public class WorkItemsListQueryResult
{
    public string QueryType { get; set; } = "";
    public string QueryResultType { get; set; } = "";
    public DateTime AsOf { get; set; }
    public WorkItemsListQueryColumn[] Columns { get; set; } = [];
    public WorkItemsListQueryItem[] WorkItems { get; set; } = [];
}

public class WorkItemsListQueryColumn
{
    public string ReferenceName { get; set; } = "";
    public string Name { get; set; } = "";
    public string Url { get; set; } = "";
}

public class WorkItemsListQueryItem
{
    public int Id { get; set; } = 0;
    public string Url { get; set; } = "";
}

public class WorkItem
{
    public int Id { get; set; }
    public int Rev { get; set; }
    public Fields Fields { get; set; } = new();
    public Relations[] Relations { get; set; } = [];
    public string Url { get; set; } = "";
}

public class Fields
{
    [JsonPropertyName("System.AreaPath")]
    public string SystemAreaPath { get; set; } = "";
    [JsonPropertyName("System.TeamProject")]
    public string SystemTeamProject { get; set; } = "";
    [JsonPropertyName("System.IterationPath")]
    public string SystemIterationPath { get; set; } = "";
    [JsonPropertyName("System.WorkItemType")]
    public string SystemWorkItemType { get; set; } = "";
    [JsonPropertyName("System.State")]
    public string SystemState { get; set; } = "";
    [JsonPropertyName("System.Reason")]
    public string SystemReason { get; set; } = "";
    [JsonPropertyName("System.CreatedDate")]
    public DateTime SystemCreatedDate { get; set; }
    [JsonPropertyName("System.CreatedBy")]
    public TeamMember SystemCreatedBy { get; set; } = new();
    [JsonPropertyName("System.ChangedDate")]
    public DateTime SystemChangedDate { get; set; }
    [JsonPropertyName("System.ChangedBy")]
    public TeamMember SystemChangedBy { get; set; } = new();
    [JsonPropertyName("System.Title")]
    public string SystemTitle { get; set; } = "";
    [JsonPropertyName("Microsoft.VSTS.Scheduling.Effort")]
    public float MicrosoftVSTSSchedulingEffort { get; set; }
    [JsonPropertyName("System.Description")]
    public string SystemDescription { get; set; } = "";
    [JsonPropertyName("System.AssignedTo")]
    public TeamMember SystemAssignedTo { get; set; } = new();
    [JsonPropertyName("System.BoardLane")]
    public string SystemBoardLane { get; set; } = "";
    [JsonPropertyName("Microsoft.VSTS.Scheduling.RemainingWork")]
    public float MicrosoftVSTSSchedulingRemainingWork { get; set; }
    [JsonPropertyName("Microsoft.VSTS.Common.Priority")]
    public float MicrosoftVSTSCommonPriority { get; set; }
    [JsonPropertyName("System.Tags")]
    public string SystemTags { get; set; } = "";
    [JsonPropertyName("Microsoft.VSTS.TCM.Steps")]
    public string MicrosoftVSTSTCMSteps { get; set; } = "";
    [JsonPropertyName("Microsoft.VSTS.TCM.Parameters")]
    public string MicrosoftVSTSTCMParameters { get; set; } = "";
    [JsonPropertyName("Microsoft.VSTS.TCM.LocalDataSource")]
    public string MicrosoftVSTSTCMLocalDataSource { get; set; } = "";
    [JsonPropertyName("Microsoft.VSTS.TCM.AutomationStatus")]
    public string MicrosoftVSTSTCMAutomationStatus { get; set; } = "";
    [JsonPropertyName("Microsoft.VSTS.Common.AcceptanceCriteria")]
    public string MicrosoftVSTSCommonAcceptanceCriteria { get; set; } = "";
}

public class Relations
{
    public string Rel { get; set; } = "";
    public string Url { get; set; } = "";
    public Dictionary<string, string> Attributes { get; set; } = new();
}

[JsonConverter(typeof(JsonStringEnumConverter<WorkItemExpand>))]
public enum WorkItemExpand
{
    None,
    Relations,
    Fields,
    Links,
    All
}

[JsonConverter(typeof(JsonStringEnumConverter<WorkItemErrorPolicy>))]
public enum WorkItemErrorPolicy
{
    Fail,
    Omit
}

public class WorkItemsQueryRequest
{
    [JsonPropertyName("$expand")]
    public WorkItemExpand Expand { get; set; } = WorkItemExpand.None;
    public WorkItemErrorPolicy ErrorPolicy { get; set; } = WorkItemErrorPolicy.Fail;
    public DateTime AsOf { get; set; }
    public string[] Fields { get; set; } = [];
    public int[] Ids { get; set; } = [];
}

public class WorkItems
{
    public int Count { get; set; }
    public WorkItem[] Value { get; set; } = [];
}
