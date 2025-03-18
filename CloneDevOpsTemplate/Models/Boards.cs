using System.Text.Json.Serialization;

namespace CloneDevOpsTemplate.Models;

public static class BoardColumnType
{
    public const string InProgress = "inProgress";
    public const string Incoming = "incoming";
    public const string Outgoing = "outgoing";
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

public class BoardRow
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public class BoardRows
{
    public int Count { get; set; }
    public BoardRow[] Value { get; set; } = [];
}

public class StateMappings
{
    [JsonPropertyName("Product Backlog Item")]
    public string PBI { get; set; } = string.Empty;
    [JsonPropertyName("Bug")]
    public string Bug { get; set; } = string.Empty;
    [JsonPropertyName("Epic")]
    public string Epic { get; set; } = string.Empty;
    [JsonPropertyName("Feature")]
    public string Feature { get; set; } = string.Empty;
    [JsonPropertyName("User Story")]
    public string UserStory { get; set; } = string.Empty;
}
