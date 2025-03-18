using Newtonsoft.Json;

namespace CloneDevOpsTemplate.Models;

public enum BoardColumnType
{
    inProgress,
    incoming,
    outgoing
}

public class Board
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class Boards
{
    public int Count { get; set; }
    public Board[] Value { get; set; } = [];
}

public class BoardStateMapping
{
    public string Issue { get; set; } = string.Empty;
}

public class BoardColumn
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ItemLimit { get; set; }
    public BoardStateMapping StateMappings { get; set; } = new();
    //public BoardColumnType ColumnType { get; set; } = BoardColumnType.InProgress;
    public string ColumnType { get; set; } = string.Empty;
    public bool IsSplit { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class BoardColumns
{
    public int Count { get; set; }
    public BoardColumn[] Value { get; set; } = [];
}

public class StateMappings
{
    [JsonProperty(PropertyName = "Product Backlog Item")]
    public string PBI { get; set; } = string.Empty;
    [JsonProperty(PropertyName = "Bug")]
    public string bug { get; set; } = string.Empty;
    [JsonProperty(PropertyName = "Epic")]
    public string epic { get; set; } = string.Empty;
    [JsonProperty(PropertyName = "Feature")]
    public string feature { get; set; } = string.Empty;
    [JsonProperty(PropertyName = "User Story")]
    public string UserStory { get; set; } = string.Empty;
}
