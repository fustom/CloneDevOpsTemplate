using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;

namespace CloneDevOpsTemplate.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProjectService _projectService;
    private readonly IIterationService _iterationService;

    public HomeController(ILogger<HomeController> logger, IProjectService projectService, IIterationService iterationService)
    {
        _logger = logger;
        _projectService = projectService;
        _iterationService = iterationService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    [Route("Home/Iterations/{projectId}")]
    async public Task<IActionResult> Iterations(Guid projectId)
    {
        Iterations iterations = await _iterationService.GetIterationsAsync(projectId) ?? new Iterations();
        return View(iterations.Value);
    }


    [HttpGet]
    [Route("Home/Projects")]
    async public Task<IActionResult> Projects()
    {
        Projects projects = await _projectService.GetAllProjectsAsync() ?? new Projects();
        return View(projects.Value);
    }

    [HttpGet]
    [Route("Home/Project/{projectId}")]
    async public Task<IActionResult> Project(string projectId)
    {
        Project project = await _projectService.GetProjectAsync(projectId) ?? new Project();
        return View(project);
    }

    [HttpGet]
    [Route("Home/ProjectProperties/{projectId}")]
    async public Task<IActionResult> ProjectProperties(string projectId)
    {
        ProjectProperties projectProperties = await _projectService.GetProjectPropertiesAsync(projectId) ?? new ProjectProperties();
        //string processTemplateType = projectProperties.Value.Where(x => x.Name == "System.ProcessTemplateType").FirstOrDefault()?.Value.ToString() ?? string.Empty;
        return View(projectProperties.Value);
    }

    [HttpPost]
    [Route("Home/Login")]
    public IActionResult Login(LoginModel loginModel)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // update session with user credentials
                HttpContext.Session.SetString(Const.SessionKeyOrganizationName, loginModel.OrganizationName);
                HttpContext.Session.SetString(Const.SessionKeyAccessToken, loginModel.AccessToken);

                return Redirect("Projects");
            }
            catch (Exception)
            {
                return Redirect("Error");
            }
        }

        return View("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
