using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.IServices;

public interface IServiceService
{
    Task<ServicesModel?> GetServicesAsync(Guid projectId);
    Task<ServiceModel?> CreateServiceAsync(string serviceName, string templateRepositoryRemoteUrl, string endpointName, Guid projectId);
}
