using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public interface IProjectService
{
    Task<Processes?> GetProcessesAsync();
    Task<Process?> GetProcessAsync(string processTemplateType);
    Task<Projects?> GetAllProjectsAsync();
    Task<Project?> GetProjectAsync(Guid projectId);
    Task<ProjectProperties?> GetProjectPropertiesAsync(Guid projectId);
    Task<CreateProjectResponse?> CreateProjectAsync(string processTemplateType, string name = "New Project", string sourceControlType = "Git", string description = "New Project Description");
}
