using System.Text.Json.Serialization;

namespace CloneDevOpsTemplate.Models;

[JsonConverter(typeof(JsonStringEnumConverter<BoardColumnType>))]
public enum BoardColumnType
{
    InProgress,
    Incoming,
    Outgoing
}

public class BoardValue
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class Boards
{
    public int Count { get; set; }
    public BoardValue[] Value { get; set; } = [];
}

public class Board
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public bool CanEdit { get; set; }
    public BoardColumn[] Columns { get; set; } = [];
    public BoardRow[] Rows { get; set; } = [];
    public int Revision { get; set; }
}

public class StateMappings
{
    [JsonPropertyName("Product Backlog Item")]
    public string? PBI { get; set; }
    [JsonPropertyName("Bug")]
    public string? Bug { get; set; }
    [JsonPropertyName("Epic")]
    public string? Epic { get; set; }
    [JsonPropertyName("Feature")]
    public string? Feature { get; set; }
    [JsonPropertyName("User Story")]
    public string? UserStory { get; set; }
    [JsonPropertyName("Issue")]
    public string? Issue { get; set; }
}

public class BoardColumn
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int ItemLimit { get; set; }
    public StateMappings StateMappings { get; set; } = new();
    public BoardColumnType ColumnType { get; set; } = BoardColumnType.InProgress;
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
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Color { get; set; }
}

public class BoardRows
{
    public int Count { get; set; }
    public BoardRow[] Value { get; set; } = [];
}

public class ViewBoard
{
    public Board Board { get; set; } = new();
    public Guid ProjectId { get; set; }
    public Guid TeamId { get; set; }
}