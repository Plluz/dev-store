using DevStore.WebApp.MVC.Models;
using DevStore.WebApp.MVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DevStore.WebApp.MVC.Controllers
{
    public class PedidoController : MainController
    {
        private readonly IClienteService _clienteService;
        private readonly IComprasBffService _comprasBffService;

        public PedidoController(IClienteService clienteService,
            IComprasBffService comprasBffService)
        {
            _clienteService = clienteService;
            _comprasBffService = comprasBffService;
        }

        [HttpGet]
        [Route("endereco-de-entrega")]
        public async Task<IActionResult> EnderecoEntrega()
        {
            var carrinho = await _comprasBffService.GetShoppingCart();
            if (carrinho.Items.Count == 0) return RedirectToAction("Index", "ShoppingCart");

            var endereco = await _clienteService.ObterEndereco();
            var pedido = _comprasBffService.MapearParaPedido(carrinho, endereco);

            return View(pedido);
        }

        [HttpGet]
        [Route("pagamento")]
        public async Task<IActionResult> Pagamento()
        {
            var carrinho = await _comprasBffService.GetShoppingCart();
            if (carrinho.Items.Count == 0) return RedirectToAction("Index", "ShoppingCart");

            var pedido = _comprasBffService.MapearParaPedido(carrinho, null);

            return View(pedido);
        }

        [HttpPost]
        [Route("finalizar-pedido")]
        public async Task<IActionResult> FinalizarPedido(PedidoTransacaoViewModel pedidoTransacao)
        {
            if (!ModelState.IsValid) return View("Pagamento", _comprasBffService.MapearParaPedido(
                await _comprasBffService.GetShoppingCart(), null));

            var retorno = await _comprasBffService.FinalizarPedido(pedidoTransacao);

            if (ResponsePossuiErros(retorno))
            {
                var carrinho = await _comprasBffService.GetShoppingCart();
                if (carrinho.Items.Count == 0) return RedirectToAction("Index", "ShoppingCart");

                var pedidoMap = _comprasBffService.MapearParaPedido(carrinho, null);
                return View("Pagamento", pedidoMap);
            }

            return RedirectToAction("PedidoConcluido");
        }

        [HttpGet]
        [Route("pedido-concluido")]
        public async Task<IActionResult> PedidoConcluido()
        {
            return View("ConfirmacaoPedido", await _comprasBffService.ObterUltimoPedido());
        }

        [HttpGet("meus-pedidos")]
        public async Task<IActionResult> MeusPedidos()
        {
            return View(await _comprasBffService.ObterListaPorClienteId());
        }
    }
}