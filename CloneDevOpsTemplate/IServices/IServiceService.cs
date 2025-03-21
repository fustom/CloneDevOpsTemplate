using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public interface IServiceService
{
    Task<ServicesModel?> GetServicesAsync(Guid projectId);
    Task<ServiceModel?> CreateServiceAsync(string serviceName, string templateRepositoryRemoteUrl, string endpointName, Guid projectId);
}
