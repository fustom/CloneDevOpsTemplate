using CloneDevOpsTemplate.IServices;
using CloneDevOpsTemplate.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

[Authorize]
public class ServiceController(IServiceService serviceService) : Controller
{
    private readonly IServiceService _serviceService = serviceService;

    public async Task<IActionResult> ProjectServices(Guid projectId)
    {
        ServicesModel services = new();

        if (!ModelState.IsValid)
        {
            return View("Services", services.Value);
        }

        services = await _serviceService.GetServicesAsync(projectId) ?? new();
        return View("Services", services.Value);
    }
}