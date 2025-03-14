namespace CloneDevOpsTemplate.Models;

public class ErrorResponse
{
    public string? Message { get; set; }
    public string? TypeName { get; set; }
    public string? TypeKey { get; set; }
    public string? InnerException { get; set; }
    public int ErrorCode { get; set; }
    public int EventId { get; set; }
}