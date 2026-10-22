using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.IServices;

public interface IPolicyService
{
    Task<PolicyConfigurations?> GetPolicyConfigurationsAsync(Guid projectId);
    Task<PolicyConfiguration?> CreatePolicyConfigurationAsync(Guid projectId, PolicyConfiguration policyConfiguration);
}
