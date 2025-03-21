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

public class BoardCards
{
    public Cards Cards { get; set; } = new Cards();
    public string BoardName { get; set; } = string.Empty;
}
