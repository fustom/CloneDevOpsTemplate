namespace CloneDevOpsTemplate.Models;

public class TestPlans
{
    public int Count { get; set; }
    public TestPlan[] Value { get; set; } = [];
}

public class TestPlan
{
    public int Id { get; set; }
    public Project Project { get; set; } = new();
    public TestSuiteReference RootSuite { get; set; } = new();
    public int Revision { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AreaPath { get; set; } = string.Empty;
    public string Iterations { get; set; } = string.Empty;
    public IdentityRef Owner { get; set; } = new();
    public string State { get; set; } = string.Empty;
}

public class TestSuiteReference
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}