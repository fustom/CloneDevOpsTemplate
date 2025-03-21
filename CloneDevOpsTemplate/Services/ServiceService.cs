using System.Text.Json;
using CloneDevOpsTemplate.Constants;
using CloneDevOpsTemplate.Models;

namespace CloneDevOpsTemplate.Services;

public class ServiceService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor) : IServiceService
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("DevOpsServer");
    private readonly string accessToken = httpContextAccessor?.HttpContext?.Session.GetString(Const.SessionKeyAccessToken) ?? string.Empty;
    public Task<ServicesModel?> GetServicesAsync(Guid projectId)
    {
        return _client.GetFromJsonAsync<ServicesModel>($"{projectId}/_apis/serviceendpoint/endpoints");
    }

    public async Task<ServiceModel?> CreateServiceAsync(string serviceName, string templateRepositoryRemoteUrl, string endpointName, Guid projectId)
    {
        ServiceModelBase serviceModelBase = new()
        {
            Name = serviceName,
            Type = "git",
            Url = templateRepositoryRemoteUrl,
            Authorization = new()
            {
                Scheme = "UsernamePassword",
                Parameters = new()
                {
                    UserName = "",
                    Password = accessToken
                }
            },
            IsReady = true,
            ServiceEndpointProjectReferences =
            [
                new ServiceEndpointProjectReference
                {
                    ProjectReference = new()
                    {
                        Id = projectId
                    },
                    Name = endpointName
                }
            ]
        };
        var resp = await _client.PostAsJsonAsync($"_apis/serviceendpoint/endpoints?api-version=7.1", serviceModelBase, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return await resp.Content.ReadFromJsonAsync<ServiceModel>();
    }
}