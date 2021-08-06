using DevStore.Bff.Compras.Extensions;
using DevStore.Bff.Compras.Models;
using DevStore.Core.Communication;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DevStore.Bff.Compras.Services
{
    public interface IPedidoService
    {
        Task<ResponseResult> FinishOrder(PedidoDTO pedido);
        Task<PedidoDTO> GetLastOrder();
        Task<IEnumerable<PedidoDTO>> GetClientsByClientId();

        Task<VoucherDTO> GetVoucherByCode(string codigo);
    }

    public class PedidoService : Service, IPedidoService
    {
        private readonly HttpClient _httpClient;

        public PedidoService(HttpClient httpClient, IOptions<AppServicesSettings> settings)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(settings.Value.PedidoUrl);
        }

        public async Task<ResponseResult> FinishOrder(PedidoDTO pedido)
        {
            var pedidoContent = GetContent(pedido);

            var Response = await _httpClient.PostAsync("/pedido/", pedidoContent);

            if (!ManageHttpResponse(Response)) return await DeserializeResponse<ResponseResult>(Response);

            return Ok();
        }

        public async Task<PedidoDTO> GetLastOrder()
        {
            var Response = await _httpClient.GetAsync("/pedido/ultimo/");

            if (Response.StatusCode == HttpStatusCode.NotFound) return null;

            ManageHttpResponse(Response);

            return await DeserializeResponse<PedidoDTO>(Response);
        }

        public async Task<IEnumerable<PedidoDTO>> GetClientsByClientId()
        {
            var Response = await _httpClient.GetAsync("/pedido/lista-cliente/");

            if (Response.StatusCode == HttpStatusCode.NotFound) return null;

            ManageHttpResponse(Response);

            return await DeserializeResponse<IEnumerable<PedidoDTO>>(Response);
        }

        public async Task<VoucherDTO> GetVoucherByCode(string codigo)
        {
            var Response = await _httpClient.GetAsync($"/voucher/{codigo}/");

            if (Response.StatusCode == HttpStatusCode.NotFound) return null;

            ManageHttpResponse(Response);

            return await DeserializeResponse<VoucherDTO>(Response);
        }
    }
}