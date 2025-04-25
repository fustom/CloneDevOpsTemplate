using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.IServices;

public interface ITestService
{
    Task<TestPlans?> GetTestPlansAsync(Guid projectId);
    Task<TestSuites?> GetTestSuitesAsync(Guid projectId, int testPlanId);
}
