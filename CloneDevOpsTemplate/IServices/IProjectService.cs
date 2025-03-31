using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.IServices;

public interface IProjectService
{
    Task<Processes?> GetProcessesAsync();
    Task<Process?> GetProcessAsync(string processTemplateType);
    Task<Projects?> GetAllProjectsAsync();
    Task<Project?> GetProjectAsync(Guid projectId);
    Task<Project?> GetProjectAsync(string projectName);
    Task<ProjectProperties?> GetProjectPropertiesAsync(Guid projectId);
    Task<CreateProjectResponse?> CreateProjectAsync(string name, string description, string processTemplateType, string sourceControlType, Visibility visibility);
}
