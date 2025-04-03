using System.Net.Http.Headers;
using System.Text;
using CloneDevOpsTemplate.Constants;

namespace CloneDevOpsTemplate.MessageHandlers;

public class DevOpsAuthorizationHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using ILoggerFactory loggerFactory =
            LoggerFactory.Create(builder =>
                builder.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                }));
        ILogger<Program> logger = loggerFactory.CreateLogger<Program>();

        using (logger.BeginScope("[HttpClient scope]"))
        {
            string accessToken = httpContextAccessor?.HttpContext?.Session.GetString(Const.SessionKeyAccessToken) ?? string.Empty;
            string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format(":{0}", accessToken)));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            string organizationName = httpContextAccessor?.HttpContext?.Session.GetString(Const.SessionKeyOrganizationName) ?? string.Empty;
            UriBuilder builder = new(request.RequestUri ?? new Uri(string.Empty));
            builder.Path = $"{organizationName}{builder.Path}";
            request.RequestUri = builder.Uri;

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                string requestBody = "";
                string responseBody = "";

                if (response.Content is not null)
                {
                    responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                }
                if (request.Content is not null)
                {
                    requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                }
                logger.LogInformation("[ERROR] Request: {RequestUri} RequestBody: {RequestBody} Response: {StatusCode} ResponseBody: {ResponseBody}", request.RequestUri, requestBody, response.StatusCode, responseBody);
            }
            else
            {
                logger.LogInformation("Request: {RequestUri} Response: {StatusCode}", request.RequestUri, response.StatusCode);
            }

            return response;
        }
    }
}
