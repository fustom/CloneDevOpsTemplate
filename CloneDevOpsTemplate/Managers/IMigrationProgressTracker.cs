using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Managers;

public interface IMigrationProgressTracker
{
    void Report(Guid operationId, int step, int totalSteps, string message);
    void ReportDetail(Guid operationId, string detail);
    IProgress<string> CreateDetailReporter(Guid operationId);
    MigrationProgress? Get(Guid operationId);
    void Remove(Guid operationId);
}
