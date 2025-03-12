using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;

namespace CloneDevOpsTemplate.Controllers;

public class HomeController(IIterationService iterationService) : Controller
{
    private readonly IIterationService _iterationService = iterationService;

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    async public Task<IActionResult> Iterations(Guid projectId)
    {
        Iterations iterations = await _iterationService.GetIterationsAsync(projectId) ?? new Iterations();
        return View(iterations.Value);
    }

    public IActionResult Login(LoginModel loginModel)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // update session with user credentials
                HttpContext.Session.SetString(Const.SessionKeyOrganizationName, loginModel.OrganizationName);
                HttpContext.Session.SetString(Const.SessionKeyAccessToken, loginModel.AccessToken);

                return RedirectToAction("Projects", "Project");
            }
            catch (Exception)
            {
                return Redirect("Error");
            }
        }

        return View("Index");
    }

    public IActionResult Logout()
    {
        if (ModelState.IsValid)
        {
            try
            {
                HttpContext.Session.Clear();
                ViewBag.Loggedout = true;

                return View("Index");
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
