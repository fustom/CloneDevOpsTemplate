using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class ProjectService
{
    public async void CreateProject(HttpClient client, string processTemplateType, string name = "New Project", string sourceControlType = "Git", string description = "New Project Description")
    {
        CreateProject createProject = new CreateProject
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

        await client.PostAsJsonAsync("https://dev.azure.com/{loginModel.OrganizationName}/_apis/projects", createProject);
    }
}