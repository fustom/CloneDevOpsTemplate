using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

public class ProjectController(IProjectService projectService) : Controller
{
    private readonly IProjectService _projectService = projectService;

    async public Task<IActionResult> Projects()
    {
        Projects projects = await _projectService.GetAllProjectsAsync() ?? new Projects();
        return View(projects.Value);
    }

    async public Task<IActionResult> Project(Guid projectId)
    {
        Project project = await _projectService.GetProjectAsync(projectId) ?? new Project();
        return View(project);
    }

    async public Task<IActionResult> ProjectProperties(Guid projectId)
    {
        ProjectProperties projectProperties = await _projectService.GetProjectPropertiesAsync(projectId) ?? new ProjectProperties();
        return View(projectProperties.Value);
    }

    async public Task<IActionResult> CreateProject(Guid templateProjectId, string newProjectName)
    {
        ProjectProperties projectProperties = await _projectService.GetProjectPropertiesAsync(templateProjectId) ?? new ProjectProperties();
        string processTemplateType = projectProperties.Value.Where(x => x.Name == "System.ProcessTemplateType").FirstOrDefault()?.Value.ToString() ?? string.Empty;
        CreateProjectResponse project = await _projectService.CreateProjectAsync(processTemplateType, newProjectName) ?? new CreateProjectResponse();
        //TODO: Wait for project to be created

        return RedirectToAction($"Project/{project.Id}");
    }
}