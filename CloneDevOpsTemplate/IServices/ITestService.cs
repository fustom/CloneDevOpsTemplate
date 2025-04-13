using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.IServices;

public interface ITestService
{
    Task<TestPlans?> GetTestPlansAsync(Guid projectId);
}
