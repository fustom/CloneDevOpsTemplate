using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplateTest.Managers;

public class MigrationProgressTrackerExtensionsTest
{
    private readonly MigrationProgressTracker _tracker = new();

    [Fact]
    public async Task RunAsync_Success_CompletesAndPassesDetailReporter()
    {
        var operationId = Guid.NewGuid();

        var returnedId = await _tracker.RunAsync(operationId, "Cloning things...", "Done.", progress =>
        {
            progress.Report("Working");
            return Task.FromResult(new CloneResult());
        });

        Assert.Equal(operationId, returnedId);
        var progress = _tracker.Get(operationId);
        Assert.NotNull(progress);
        Assert.Equal(MigrationStatus.Succeeded, progress.Status);
        Assert.Equal("Done.", progress.Message);
        Assert.Equal(["Step 1 of 1: Cloning things...", "Working", "Done."], progress.Details.Select(d => d.Message));
    }

    [Fact]
    public async Task RunAsync_Errors_FailsWithStepPrefix()
    {
        var operationId = await _tracker.RunAsync(Guid.NewGuid(), "Cloning things...", "Done.",
            _ => Task.FromResult(CloneResult.FromError("Boom")));

        var progress = _tracker.Get(operationId);
        Assert.Equal(MigrationStatus.Failed, progress?.Status);
        Assert.Equal("Cloning things failed: Boom", progress?.Message);
    }

    [Fact]
    public async Task RunAsync_EmptyOperationId_GeneratesOne()
    {
        var operationId = await _tracker.RunAsync(Guid.Empty, "Step", "Done.", _ => Task.FromResult(new CloneResult()));

        Assert.NotEqual(Guid.Empty, operationId);
        Assert.Equal(MigrationStatus.Succeeded, _tracker.Get(operationId)?.Status);
    }

    [Fact]
    public async Task RunAsync_Throws_RemovesProgressAndRethrows()
    {
        var operationId = Guid.NewGuid();

        await Assert.ThrowsAsync<TimeoutException>(() =>
            _tracker.RunAsync(operationId, "Step", "Done.", _ => throw new TimeoutException()));

        Assert.Null(_tracker.Get(operationId));
    }
}
