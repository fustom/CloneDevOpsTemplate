using System.Collections.Concurrent;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Managers;

public class MigrationProgressTracker : IMigrationProgressTracker
{
    public const int MaxDetails = 200;

    private readonly ConcurrentDictionary<Guid, MigrationProgress> _progress = new();

    public void Start(Guid operationId, int totalSteps)
    {
        if (operationId == Guid.Empty)
        {
            return;
        }

        _progress[operationId] = new MigrationProgress { TotalSteps = totalSteps };
    }

    public void Report(Guid operationId, int step, string message)
    {
        if (operationId == Guid.Empty)
        {
            return;
        }

        var progress = _progress.GetOrAdd(operationId, _ => new MigrationProgress());
        lock (progress)
        {
            progress.Step = step;
            // Guards against a missing or too small Start total so the UI never exceeds 100%.
            progress.TotalSteps = Math.Max(progress.TotalSteps, step);
            progress.Message = message;
            progress.CurrentDetail = null;
            AddDetail(progress, $"Step {step} of {progress.TotalSteps}: {message}");
        }
    }

    public void ReportDetail(Guid operationId, string detail)
    {
        if (operationId == Guid.Empty || string.IsNullOrWhiteSpace(detail))
        {
            return;
        }

        var progress = _progress.GetOrAdd(operationId, _ => new MigrationProgress());
        lock (progress)
        {
            progress.CurrentDetail = detail;
            AddDetail(progress, detail);
        }
    }

    public IProgress<string> CreateDetailReporter(Guid operationId) => new DetailReporter(this, operationId);

    public MigrationProgress? Get(Guid operationId)
    {
        if (!_progress.TryGetValue(operationId, out var progress))
        {
            return null;
        }

        lock (progress)
        {
            return new MigrationProgress
            {
                Step = progress.Step,
                TotalSteps = progress.TotalSteps,
                Message = progress.Message,
                CurrentDetail = progress.CurrentDetail,
                Details = [.. progress.Details]
            };
        }
    }

    public void Remove(Guid operationId) => _progress.TryRemove(operationId, out _);

    private static void AddDetail(MigrationProgress progress, string message)
    {
        progress.Details.Add(new MigrationProgressDetail { Timestamp = DateTimeOffset.UtcNow, Step = progress.Step, Message = message });
        if (progress.Details.Count > MaxDetails)
        {
            progress.Details.RemoveRange(0, progress.Details.Count - MaxDetails);
        }
    }

    // Reports synchronously, unlike System.Progress<T>, so detail ordering is preserved.
    private sealed class DetailReporter(IMigrationProgressTracker tracker, Guid operationId) : IProgress<string>
    {
        public void Report(string value) => tracker.ReportDetail(operationId, value);
    }
}
