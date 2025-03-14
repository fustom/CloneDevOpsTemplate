using System.Net;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class ProjectService(IHttpClientFactory httpClientFactory) : IProjectService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");

    public Task<Projects?> GetAllProjectsAsync()
    {
        return _client.GetFromJsonAsync<Projects>($"_apis/projects?stateFilter=wellFormed&$top=1000");
    }

    public Task<Project?> GetProjectAsync(Guid projectId)
    {
        return _client.GetFromJsonAsync<Project>($"_apis/projects/{projectId}");
    }

    public Task<Project?> GetProjectAsync(string projectName)
    {
        return _client.GetFromJsonAsync<Project>($"_apis/projects/{projectName}");
    }

    public Task<ProjectProperties?> GetProjectPropertiesAsync(Guid projectId)
    {
        return _client.GetFromJsonAsync<ProjectProperties>($"_apis/projects/{projectId}/properties");
    }

    public Task<Processes?> GetProcessesAsync()
    {
        return _client.GetFromJsonAsync<Processes>($"_apis/work/processes");
    }

    public Task<Process?> GetProcessAsync(string processTemplateType)
    {
        return _client.GetFromJsonAsync<Process>($"_apis/work/processes/{processTemplateType}");
    }

    public async Task<CreateProjectResponse?> CreateProjectAsync(string name = "New Project", string description = "New Project Description", string processTemplateType = "Basic", string sourceControlType = "Git", string visibility = "private")
    {
        CreateProject createProject = new()
        {
            Name = name,
            Capabilities = new Capabilities
            {
                Versioncontrol = new VersionControl
                {
                    SourceControlType = sourceControlType
                },
                ProcessTemplate = new ProcessTemplate
                {
                    TemplateTypeId = processTemplateType
                }
            },
            Description = description,
            Visibility = visibility
        };

        var result = await _client.PostAsJsonAsync($"_apis/projects?api-version=7.1", createProject);
        if (result.IsSuccessStatusCode || result.StatusCode == HttpStatusCode.BadRequest)
        {
            return await result.Content.ReadFromJsonAsync<CreateProjectResponse>();
        }
        return null;
    }
}