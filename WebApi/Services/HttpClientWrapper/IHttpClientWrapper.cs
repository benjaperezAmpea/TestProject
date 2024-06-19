using System.Threading.Tasks;

namespace WebApi.Services.HttpClientWrapper
{
    public interface IHttpClientWrapper
    {
        Task<string> GetStringAsync(string requestUri);
    }
}
