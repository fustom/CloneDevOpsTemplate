namespace CloneDevOpsTemplate.Models;

public class IdentityRef
{
    public string DisplayName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Descriptor { get; set; } = string.Empty;
    public string DirectoryAlias { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public bool IsDeletedInOrigin { get; set; }
}
