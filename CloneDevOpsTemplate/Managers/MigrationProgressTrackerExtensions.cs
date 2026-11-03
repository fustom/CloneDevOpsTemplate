using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Managers;

public static class MigrationProgressTrackerExtensions
{
    // The result page reads the outcome from the tracker, so every run needs an operation id.
    public static Guid EnsureOperationId(Guid operationId) => operationId == Guid.Empty ? Guid.NewGuid() : operationId;

    /// <summary>
    /// Runs a single-step clone, records its outcome and returns the operation id to show the result with.
    /// </summary>
    public static async Task<Guid> RunAsync(this IMigrationProgressTracker tracker, Guid operationId, string stepMessage, string successMessage, Func<IProgress<string>, Task<CloneResult>> clone)
    {
        operationId = EnsureOperationId(operationId);
        tracker.Start(operationId, 1);
        tracker.Report(operationId, 1, stepMessage);

        CloneResult result;
        try
        {
            result = await clone(tracker.CreateDetailReporter(operationId));
        }
        catch
        {
            tracker.Remove(operationId);
            throw;
        }

        if (result.Succeeded)
        {
            tracker.Complete(operationId, successMessage);
        }
        else
        {
            tracker.Fail(operationId, $"{stepMessage.TrimEnd('.')} failed: {result.ErrorMessage}");
        }

        return operationId;
    }
}
