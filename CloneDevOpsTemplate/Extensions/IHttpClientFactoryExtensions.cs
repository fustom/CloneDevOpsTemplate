using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CloneDevOpsTemplate.Controllers;

namespace CloneDevOpsTemplate.Extensions
{
    public static class IHttpClientFactoryExtensions
    {
        public static HttpClient CreateClientWithCredentials(this IHttpClientFactory factory, string accessToken)
        {        
            HttpClient client = factory.CreateClient();
            string _credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format(":{0}", accessToken)));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", _credentials);

            return client;
        }
    }
}