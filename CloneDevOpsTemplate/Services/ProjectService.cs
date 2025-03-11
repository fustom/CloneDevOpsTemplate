using CloneDevOpsTemplate.Controllers;
using CloneDevOpsTemplate.Extensions;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class ProjectService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor) : IProjectService
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

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

        var orgName = _httpContextAccessor?.HttpContext?.Session.GetString(HomeController.SessionKeyOrganizationName);
        var accToken = _httpContextAccessor?.HttpContext?.Session.GetString(HomeController.SessionKeyAccessToken);

        return _httpClientFactory.CreateClientWithCredentials(accToken).PostAsJsonAsync($"https://dev.azure.com/{orgName}/_apis/projects", createProject);
    }
}