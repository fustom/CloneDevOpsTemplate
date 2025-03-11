using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CloneDevOpsTemplate.Models;
using System.Net.Http.Headers;
using System.Text;
using CloneDevOpsTemplate.Services;

namespace CloneDevOpsTemplate.Controllers;

public class HomeController : Controller
{
    public const string SessionKeyOrganizationName = "OrganizationName";
    public const string SessionKeyAccessToken = "AccessToken";
    
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IProjectService _projectService;

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, IProjectService projectService)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _projectService = projectService;
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
    async public Task<IActionResult> Login(LoginModel loginModel)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // update session with user credentials
                var orgName = loginModel.OrganizationName;
                var accToken = loginModel.AccessToken;
                HttpContext.Session.SetString(SessionKeyOrganizationName, orgName);
                HttpContext.Session.SetString(SessionKeyAccessToken, accToken);
                
                HttpClient client = _httpClientFactory.CreateClient();
                string _credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format(":{0}", accToken)));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

                string requestUri = string.Empty;
                // requestUri = $"https://dev.azure.com/{orgName}/_apis/projects?stateFilter=wellFormed&$top=1000";
                // Projects projects = await client.GetFromJsonAsync<Projects>(requestUri) ?? new Projects();
                // return View("Projects", projects.Value);                

                string id = "454fb150-83f5-4336-b491-1cfe5bf9739b";
                requestUri = $"https://dev.azure.com/{orgName}/_apis/projects/{id}/properties";
                ProjectProperties projectProperties = await client.GetFromJsonAsync<ProjectProperties>(requestUri) ?? new ProjectProperties();
                
                string processTemplateType = projectProperties.Value.Where(x => x.Name == "System.ProcessTemplateType").FirstOrDefault()?.Value.ToString() ?? string.Empty;

                await _projectService.CreateProjectAsync(processTemplateType);

                // requestUri = $"https://dev.azure.com/{orgName}/_apis/work/processes/{processTemplateType}";
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
