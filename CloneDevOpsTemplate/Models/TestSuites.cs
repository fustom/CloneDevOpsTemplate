namespace CloneDevOpsTemplate.Models;

public class TestSuites
{
    public int Count { get; set; }
    public TestSuite[] Value { get; set; } = [];
}

public class TestSuite
{
    public int Id { get; set; }
    public Project Project { get; set; } = new();
    public TestPlanReference Plan { get; set; } = new();
    public int Revision { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool HasChildren { get; set; }
    public TestSuite[] Children { get; set; } = [];
    public string SuiteType { get; set; } = string.Empty;
    public TestSuiteReference ParentSuite { get; set; } = new();
    public bool InheritDefaultConfigurations { get; set; }
    public TestConfigurationReference[] DefaultConfigurations { get; set; } = [];
    public IdentityRef[] DefaultTesters { get; set; } = [];
}

public class TestPlanReference
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class TestConfigurationReference
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
