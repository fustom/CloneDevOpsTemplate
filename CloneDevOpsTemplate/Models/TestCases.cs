using System.Text.Json.Serialization;

namespace CloneDevOpsTemplate.Models;

public class TestCases
{
    public int Count { get; set; }
    public TestCase[] Value { get; set; } = [];
}

public class TestCase
{
    public int Order { get; set; }
    public PointAssignment[] PointAssignments { get; set; } = [];
    public Project Project { get; set; } = new();
    public TestPlanReference TestPlan { get; set; } = new();
    public TestSuiteReference TestSuite { get; set; } = new();
    public WorkItemDetails WorkItem { get; set; } = new();
}

public class PointAssignment
{
    public int Id { get; set; }
    public int ConfigurationId { get; set; }
    public string ConfigurationName { get; set; } = string.Empty;
    public IdentityRef Tester { get; set; } = new();
}

public class WorkItemDetails
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
