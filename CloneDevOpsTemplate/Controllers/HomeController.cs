using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Constants;

namespace CloneDevOpsTemplate.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewBag.LoginMessage = "Please login to continue.";
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Login()
    {
        ViewBag.LoginMessage = "Please login to continue.";
        return View("Index");
    }

    [HttpPost]
    public IActionResult Login(LoginModel loginModel)
    {
        if (ModelState.IsValid)
        {
            // update session with user credentials
            HttpContext.Session.SetString(Const.SessionKeyOrganizationName, loginModel.OrganizationName);
            HttpContext.Session.SetString(Const.SessionKeyAccessToken, loginModel.AccessToken);

            return RedirectToAction("Projects", "Project");
        }

        ViewBag.LoginMessage = "Login failed. Please try again.";
        return View("Index");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        ViewBag.LoginMessage = "You have been logged out.";

        return View("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
