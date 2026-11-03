using CloneDevOpsTemplate.Managers;

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
}
