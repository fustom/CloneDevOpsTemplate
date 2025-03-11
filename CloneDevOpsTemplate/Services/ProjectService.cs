using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class ProjectService(IHttpClientFactory httpClientFactory) : IProjectService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

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

        return _httpClientFactory.CreateClient().PostAsJsonAsync("https://dev.azure.com/{loginModel.OrganizationName}/_apis/projects", createProject);
    }
}