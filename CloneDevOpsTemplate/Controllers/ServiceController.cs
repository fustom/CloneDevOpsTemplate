using CloneDevOpsTemplate.Models;
using CloneDevOpsTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace CloneDevOpsTemplate.Controllers;

public class ServiceController(IServiceService serviceService) : Controller
{
    private readonly IServiceService _serviceService = serviceService;

    async public Task<IActionResult> ProjectServices(Guid projectId)
    {
        ServicesModel services = await _serviceService.GetServicesAsync(projectId) ?? new();
        return View("Services", services.Value);
    }
}