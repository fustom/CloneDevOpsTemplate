using System.Net.Http.Headers;
using System.Text;

namespace CloneDevOpsTemplate.Services;

public class DevOpsAuthorizationHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string accessToken = httpContextAccessor?.HttpContext?.Session.GetString(Const.SessionKeyAccessToken) ?? string.Empty;
        string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format(":{0}", accessToken)));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        string organizationName = httpContextAccessor?.HttpContext?.Session.GetString(Const.SessionKeyOrganizationName) ?? string.Empty;
        UriBuilder builder = new(request.RequestUri ?? new Uri(string.Empty));
        builder.Path = $"{organizationName}{builder.Path}";
        request.RequestUri = builder.Uri;

        return await base.SendAsync(request, cancellationToken);
    }
}
