namespace CloneDevOpsTemplate.Models;

public class IdentityRef
{
    public string DisplayName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Descriptor { get; set; } = string.Empty;
    public string DirectoryAlias { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public bool IsDeletedInOrigin { get; set; }
    public string UniqueName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}
