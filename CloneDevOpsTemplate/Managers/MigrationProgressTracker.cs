using System.Collections.Concurrent;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Managers;

public interface IMigrationProgressTracker
{
    void Report(Guid operationId, int step, int totalSteps, string message);
    MigrationProgress? Get(Guid operationId);
    void Remove(Guid operationId);
}

public class MigrationProgressTracker : IMigrationProgressTracker
{
    private readonly ConcurrentDictionary<Guid, MigrationProgress> _progress = new();

    public void Report(Guid operationId, int step, int totalSteps, string message)
    {
        if (operationId == Guid.Empty)
        {
            return;
        }

        _progress[operationId] = new MigrationProgress { Step = step, TotalSteps = totalSteps, Message = message };
    }

    public MigrationProgress? Get(Guid operationId) => _progress.GetValueOrDefault(operationId);

    public void Remove(Guid operationId) => _progress.TryRemove(operationId, out _);
}
