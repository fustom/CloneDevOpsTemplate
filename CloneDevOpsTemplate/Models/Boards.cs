namespace CloneDevOpsTemplate.Models;

public enum BoardColumnType
{
    Incoming = 0,
    InProgress = 1,
    Outgoing = 2
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
    public string Issue { get; set; } = string.Empty;   //TODO: check in the tool

}

public class BoardColumn
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ItemLimit { get; set; }
    public BoardColumnType ColumnType { get; set; }   //TODO: check in the tool
}

public class BoardColumns
{
    public int Count { get; set; }
    public BoardColumn[] Value { get; set; } = [];
}