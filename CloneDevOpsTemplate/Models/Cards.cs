using System.Text.Json.Serialization;

namespace CloneDevOpsTemplate.Models;

public class CardItem
{
    public string FieldIdentifier { get; set; } = string.Empty;
    public string DisplayFormat { get; set; } = string.Empty;
}

public class Cards
{
    [JsonPropertyName("Epic")]
    public IList<CardItem> Epic { get; set; } = [];
    [JsonPropertyName("Feature")]
    public IList<CardItem> Feature { get; set; } = [];
    [JsonPropertyName("Product Backlog Item")]
    public IList<CardItem> ProductBacklogItem { get; set; } = [];
    [JsonPropertyName("User Story")]
    public IList<CardItem> UserStory { get; set; } = [];
    [JsonPropertyName("Bug")]
    public IList<CardItem> Bug { get; set; } = [];
    [JsonPropertyName("Issue")]
    public IList<CardItem> Issue { get; set; } = [];
}

public class CardSettings
{
    public Cards Cards { get; set; } = new Cards();
}

public class Clause
{
    public string FieldName { get; set; } = string.Empty;
    public int Index { get; set; }
    public string LogicalOperator { get; set; } = string.Empty;
    [JsonPropertyName("operator")]
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class Settings
{
    [JsonPropertyName("title-color")]
    public string TitleColor { get; set; } = string.Empty;
    [JsonPropertyName("background-color")]
    public string BackgroundColor { get; set; } = string.Empty;
}

public class Fill
{
    public string Name { get; set; } = string.Empty;
    public string IsEnabled { get; set; } = string.Empty;
    public string Filter { get; set; } = string.Empty;
    public IList<Clause> Clauses { get; set; } = [];
    public Settings Settings { get; set; } = new Settings();
}

public class TagStyle
{
    public string Name { get; set; } = string.Empty;
    public string IsEnabled { get; set; } = string.Empty;
    public Settings Settings { get; set; } = new Settings();
}

public class Rules
{
    public IList<Fill> Fill { get; set; } = [];
    public IList<TagStyle> TagStyle { get; set; } = [];
}

public class CardStyles
{
    public string Url { get; set; } = string.Empty;
    public Rules Rules { get; set; } = new Rules();
    public string _links { get; set; } = string.Empty;
}
