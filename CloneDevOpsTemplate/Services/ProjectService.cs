using CloneDevOpsTemplate.Extensions;
using System.Text;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class ProjectService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor) : IProjectService
{
    private readonly HttpClient _client = httpClientFactory.CreateClientWithCredentials(httpContextAccessor?.HttpContext?.Session.GetString(Const.SessionKeyAccessToken) ?? string.Empty);
    private readonly string _organizationName = httpContextAccessor?.HttpContext?.Session.GetString(Const.SessionKeyOrganizationName) ?? string.Empty;

    public Task<Projects?> GetAllProjectsAsync()
    {
        return _client.GetFromJsonAsync<Projects>($"https://dev.azure.com/{_organizationName}/_apis/projects?stateFilter=wellFormed&$top=1000");
    }

    public Task<Project?> GetProjectAsync(string projectId)
    {
        return _client.GetFromJsonAsync<Project>($"https://dev.azure.com/{_organizationName}/_apis/projects/{projectId}");
    }

    public Task<ProjectProperties?> GetProjectPropertiesAsync(string projectId)
    {
        return _client.GetFromJsonAsync<ProjectProperties>($"https://dev.azure.com/{_organizationName}/_apis/projects/{projectId}/properties");
    }

    public Task<Processes?> GetProcessesAsync()
    {
        return _client.GetFromJsonAsync<Processes>($"https://dev.azure.com/{_organizationName}/_apis/work/processes");
    }

    public Task<Process?> GetProcessAsync(string processTemplateType)
    {
        return _client.GetFromJsonAsync<Process>($"https://dev.azure.com/{_organizationName}/_apis/work/processes/{processTemplateType}");
    }

    public Task<HttpResponseMessage> CreateProjectAsync(string processTemplateType, string name = "New Project", string sourceControlType = "Git", string description = "New Project Description")
    {
        CreateProject createProject = new()
        {
            Name = name,
            Capabilities = new Capabilities
            {
                VersionControl = new VersionControl
                {
                    SourceControlType = sourceControlType
                },
                ProcessTemplate = new ProcessTemplate
                {
                    TemplateTypeId = processTemplateType
                }
            },
            Description = description
        };

        return _client.PostAsJsonAsync($"https://dev.azure.com/{_organizationName}/_apis/projects", createProject);
    }
}