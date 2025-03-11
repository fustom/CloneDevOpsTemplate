using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public interface IProjectService
{
    Task<Projects?> GetAllProjectsAsync();
    Task<ProjectProperties?> GetProjectPropertiesAsync(string projectId);
    Task<HttpResponseMessage> CreateProjectAsync(string processTemplateType, string name = "New Project", string sourceControlType = "Git", string description = "New Project Description");
}
