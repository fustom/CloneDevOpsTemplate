using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CloneDevOpsTemplate.Models;
using System.Net.Http.Headers;
using System.Text;
using CloneDevOpsTemplate.Services;

namespace CloneDevOpsTemplate.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpPost]
    async public Task<IActionResult> Login(LoginModel loginModel, HttpClient client)
    {
        if (ModelState.IsValid)
        {
            try
            {
                string _credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format(":{0}", loginModel.AccessToken))); ;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                string requestUri = string.Empty;
                // requestUri = $"https://dev.azure.com/{loginModel.OrganizationName}/_apis/projects?stateFilter=wellFormed&$top=1000";
                // Projects projects = await client.GetFromJsonAsync<Projects>(requestUri) ?? new Projects();
                // return View("Projects", projects.Value);                

                string id = "454fb150-83f5-4336-b491-1cfe5bf9739b";
                requestUri = $"https://dev.azure.com/{loginModel.OrganizationName}/_apis/projects/{id}/properties";
                ProjectProperties projectProperties = await client.GetFromJsonAsync<ProjectProperties>(requestUri) ?? new ProjectProperties();
                
                string processTemplateType = projectProperties.Value.Where(x => x.Name == "System.ProcessTemplateType").FirstOrDefault()?.Value.ToString() ?? string.Empty;

                ProjectService projectService = new();
                projectService.CreateProject(client, processTemplateType);

                // requestUri = $"https://dev.azure.com/{loginModel.OrganizationName}/_apis/work/processes/{processTemplateType}";
                // Processes processes = await client.GetFromJsonAsync<Processes>(requestUri) ?? new Processes();
                
                 return View("ProjectProperties", projectProperties.Value);
            }
            catch (Exception)
            {
                return View("Index");
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
