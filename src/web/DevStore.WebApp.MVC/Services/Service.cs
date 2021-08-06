using DevStore.Core.Communication;
using DevStore.WebApp.MVC.Extensions;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DevStore.WebApp.MVC.Services
{
    public abstract class Service
    {
        protected StringContent ObterConteudo(object dado)
        {
            return new StringContent(
                JsonSerializer.Serialize(dado),
                Encoding.UTF8,
                "application/json");
        }

        protected async Task<T> DeserializeResponse<T>(HttpResponseMessage ResponseMessage)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<T>(await ResponseMessage.Content.ReadAsStringAsync(), options);
        }

        protected bool ManageResponseErrors(HttpResponseMessage Response)
        {
            switch ((int)Response.StatusCode)
            {
                case 401:
                case 403:
                case 404:
                case 500:
                    throw new CustomHttpRequestException(Response.StatusCode);

                case 400:
                    return false;
            }

            Response.EnsureSuccessStatusCode();
            return true;
        }

        protected ResponseResult RetornoOk()
        {
            return new ResponseResult();
        }
    }
}