namespace CloneDevOpsTemplate.Services;

public interface IProjectService
{
    Task<HttpResponseMessage> CreateProjectAsync(string processTemplateType, string name = "New Project", string sourceControlType = "Git", string description = "New Project Description");
}
