namespace CloneDevOpsTemplate.Models;

public class ServicesModel
{
    public int Count { get; set; }
    public ServiceModel[] Value { get; set; } = [];
}

public class ServiceModelBase
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public ServiceAuthorization Authorization { get; set; } = new();
    public bool IsReady { get; set; }
    public ServiceEndpointProjectReference[] ServiceEndpointProjectReferences { get; set; } = [];
}

public class ServiceModel : ServiceModelBase
{
    public Guid Id { get; set; }
    public IdentityRef CreatedBy { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public bool IsShared { get; set; }
    public bool IsOutdated { get; set; }
    public DateTime CreationDate { get; set; }
}

public class ServiceAuthorization
{
    public string Scheme { get; set; } = string.Empty;
    public ServiceAuthorizationParameters Parameters { get; set; } = new();
}

public class ServiceAuthorizationParameters
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class ServiceEndpointProjectReference
{
    public ProjectBase ProjectReference { get; set; } = new();
    public string Name { get; set; } = string.Empty;
}