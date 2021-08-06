using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using DevStore.Core.Communication;
using DevStore.WebApp.MVC.Extensions;
using DevStore.WebApp.MVC.Models;

namespace DevStore.WebApp.MVC.Services
{
    public interface IClienteService
    {
        Task<EnderecoViewModel> ObterEndereco();
        Task<ResponseResult> AdicionarEndereco(EnderecoViewModel endereco);
    }

    public class ClienteService : Service, IClienteService
    {
        private readonly HttpClient _httpClient;

        public ClienteService(HttpClient httpClient, IOptions<AppSettings> settings)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(settings.Value.ClienteUrl);
        }

        public async Task<EnderecoViewModel> ObterEndereco()
        {
            var Response = await _httpClient.GetAsync("/cliente/endereco/");

            if (Response.StatusCode == HttpStatusCode.NotFound) return null;

            ManageResponseErrors(Response);

            return await DeserializeResponse<EnderecoViewModel>(Response);
        }

        public async Task<ResponseResult> AdicionarEndereco(EnderecoViewModel endereco)
        {
            var enderecoContent = ObterConteudo(endereco);

            var Response = await _httpClient.PostAsync("/cliente/endereco/", enderecoContent);

            if (!ManageResponseErrors(Response)) return await DeserializeResponse<ResponseResult>(Response);

            return RetornoOk();
        }
    }
}