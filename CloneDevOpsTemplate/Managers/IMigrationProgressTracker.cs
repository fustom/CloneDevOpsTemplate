using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Managers;

public interface IMigrationProgressTracker
{
    void Start(Guid operationId, int totalSteps);
    void Report(Guid operationId, int step, string message);
    void ReportDetail(Guid operationId, string detail);
    IProgress<string> CreateDetailReporter(Guid operationId);
    void Complete(Guid operationId, string message);
    void Fail(Guid operationId, string message);
    MigrationProgress? Get(Guid operationId);
    MigrationProgress? TakeFinished(Guid operationId);
    void Remove(Guid operationId);
}
