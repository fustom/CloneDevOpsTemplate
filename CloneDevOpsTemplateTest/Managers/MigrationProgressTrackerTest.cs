using CloneDevOpsTemplate.Managers;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplateTest.Managers;

public class MigrationProgressTrackerTest
{
    private readonly MigrationProgressTracker _tracker = new();

    [Fact]
    public void Report_KeepsStepAndAppendsToDetailLog()
    {
        var operationId = Guid.NewGuid();

        _tracker.Start(operationId, 4);
        _tracker.Report(operationId, 1, "Creating project...");
        _tracker.CreateDetailReporter(operationId).Report("Waiting...");
        _tracker.Report(operationId, 2, "Cloning iterations...");

        var progress = _tracker.Get(operationId);
        Assert.NotNull(progress);
        Assert.Equal(2, progress.Step);
        Assert.Equal(4, progress.TotalSteps);
        Assert.Equal("Cloning iterations...", progress.Message);
        Assert.Null(progress.CurrentDetail);
        Assert.Equal(
            ["Step 1 of 4: Creating project...", "Waiting...", "Step 2 of 4: Cloning iterations..."],
            progress.Details.Select(d => d.Message));
        Assert.Equal([1, 1, 2], progress.Details.Select(d => d.Step));
    }

    [Fact]
    public void Start_ResetsPreviousProgress()
    {
        var operationId = Guid.NewGuid();
        _tracker.Start(operationId, 2);
        _tracker.Report(operationId, 1, "Old");

        _tracker.Start(operationId, 3);

        var progress = _tracker.Get(operationId);
        Assert.NotNull(progress);
        Assert.Equal(0, progress.Step);
        Assert.Equal(3, progress.TotalSteps);
        Assert.Empty(progress.Details);
    }

    [Fact]
    public void Report_WithoutStart_TotalStepsNeverLessThanStep()
    {
        var operationId = Guid.NewGuid();

        _tracker.Report(operationId, 2, "Step two");

        var progress = _tracker.Get(operationId);
        Assert.NotNull(progress);
        Assert.Equal(2, progress.TotalSteps);
        Assert.Equal("Step 2 of 2: Step two", progress.Details[0].Message);
    }

    [Fact]
    public void Start_EmptyOperationId_IsIgnored()
    {
        _tracker.Start(Guid.Empty, 3);

        Assert.Null(_tracker.Get(Guid.Empty));
    }

    [Fact]
    public void ReportDetail_SetsCurrentDetail()
    {
        var operationId = Guid.NewGuid();

        _tracker.Report(operationId, 1, "Step");
        _tracker.ReportDetail(operationId, "Sub-step");

        Assert.Equal("Sub-step", _tracker.Get(operationId)?.CurrentDetail);
    }

    [Fact]
    public void ReportDetail_EmptyOperationId_IsIgnored()
    {
        _tracker.ReportDetail(Guid.Empty, "Ignored");
        _tracker.CreateDetailReporter(Guid.Empty).Report("Ignored");

        Assert.Null(_tracker.Get(Guid.Empty));
    }

    [Fact]
    public void ReportDetail_TrimsLogToMaxDetails()
    {
        var operationId = Guid.NewGuid();

        for (int i = 0; i < MigrationProgressTracker.MaxDetails + 10; i++)
        {
            _tracker.ReportDetail(operationId, $"Detail {i}");
        }

        var progress = _tracker.Get(operationId);
        Assert.NotNull(progress);
        Assert.Equal(MigrationProgressTracker.MaxDetails, progress.Details.Count);
        Assert.Equal("Detail 10", progress.Details[0].Message);
    }

    [Fact]
    public void Get_ReturnsSnapshot()
    {
        var operationId = Guid.NewGuid();
        _tracker.ReportDetail(operationId, "First");

        var snapshot = _tracker.Get(operationId);
        _tracker.ReportDetail(operationId, "Second");

        Assert.Single(snapshot!.Details);
    }

    [Fact]
    public void Remove_DeletesProgress()
    {
        var operationId = Guid.NewGuid();
        _tracker.ReportDetail(operationId, "Detail");

        _tracker.Remove(operationId);

        Assert.Null(_tracker.Get(operationId));
    }

    [Fact]
    public void Complete_MarksSucceededAndAppendsMessage()
    {
        var operationId = Guid.NewGuid();
        _tracker.Start(operationId, 3);
        _tracker.Report(operationId, 1, "First");
        _tracker.ReportDetail(operationId, "Working");

        _tracker.Complete(operationId, "All done");

        var progress = _tracker.Get(operationId);
        Assert.NotNull(progress);
        Assert.Equal(MigrationStatus.Succeeded, progress.Status);
        Assert.Equal(3, progress.Step);
        Assert.Equal("All done", progress.Message);
        Assert.Null(progress.CurrentDetail);
        Assert.NotNull(progress.FinishedAt);
        Assert.Equal("All done", progress.Details[^1].Message);
    }

    [Fact]
    public void TakeFinished_ReturnsAndRemovesSucceededRun()
    {
        var operationId = Guid.NewGuid();
        _tracker.Complete(operationId, "Done");

        var progress = _tracker.TakeFinished(operationId);

        Assert.NotNull(progress);
        Assert.Equal("Done", progress.Message);
        Assert.Null(_tracker.Get(operationId));
        Assert.Null(_tracker.TakeFinished(operationId));
    }

    [Fact]
    public void TakeFinished_RunningOperation_ReturnsNullAndKeepsIt()
    {
        var operationId = Guid.NewGuid();
        _tracker.Start(operationId, 2);

        Assert.Null(_tracker.TakeFinished(operationId));
        Assert.NotNull(_tracker.Get(operationId));
    }

    [Fact]
    public void Fail_MarksFailedAndKeepsCurrentStep()
    {
        var operationId = Guid.NewGuid();
        _tracker.Start(operationId, 4);
        _tracker.Report(operationId, 2, "Second");

        _tracker.Fail(operationId, "Broken");

        var progress = _tracker.Get(operationId);
        Assert.NotNull(progress);
        Assert.Equal(MigrationStatus.Failed, progress.Status);
        Assert.Equal(2, progress.Step);
        Assert.Equal("Broken", progress.Message);
        Assert.NotNull(progress.FinishedAt);
        Assert.Equal("Broken", progress.Details[^1].Message);
    }

    [Fact]
    public void TakeFinished_ReturnsAndRemovesFailedRun()
    {
        var operationId = Guid.NewGuid();
        _tracker.Fail(operationId, "Broken");

        var progress = _tracker.TakeFinished(operationId);

        Assert.Equal(MigrationStatus.Failed, progress?.Status);
        Assert.Null(_tracker.Get(operationId));
    }

    [Fact]
    public void Complete_EmptyOperationId_IsIgnored()
    {
        _tracker.Complete(Guid.Empty, "Ignored");

        Assert.Null(_tracker.Get(Guid.Empty));
    }
}
