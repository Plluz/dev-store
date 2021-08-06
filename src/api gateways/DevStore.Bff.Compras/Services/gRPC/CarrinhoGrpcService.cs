using DevStore.Bff.Compras.Models;
using DevStore.ShoppingCart.API.Services.gRPC;
using System;
using System.Threading.Tasks;

namespace DevStore.Bff.Compras.Services.gRPC
{
    public interface ICarrinhoGrpcService
    {
        Task<ShoppingCartDto> ObterCarrinho();
    }

    public class CarrinhoGrpcService : ICarrinhoGrpcService
    {
        private readonly ShoppingCartOrders.ShoppingCartOrdersClient _carrinhoComprasClient;

        public CarrinhoGrpcService(ShoppingCartOrders.ShoppingCartOrdersClient carrinhoComprasClient)
        {
            _carrinhoComprasClient = carrinhoComprasClient;
        }

        public async Task<ShoppingCartDto> ObterCarrinho()
        {
            var Response = await _carrinhoComprasClient.GetShoppingCartAsync(new GetShoppingCartRequest());
            return MapCarrinhoClienteProtoResponseToDTO(Response);
        }

        private static ShoppingCartDto MapCarrinhoClienteProtoResponseToDTO(ShoppingCartClientClientResponse carrinhoResponse)
        {
            var carrinhoDTO = new ShoppingCartDto
            {
                Total = (decimal)carrinhoResponse.Total,
                Discount = (decimal)carrinhoResponse.Discount,
                HasVoucher = carrinhoResponse.Hasvoucher
            };

            if (carrinhoResponse.Voucher != null)
            {
                carrinhoDTO.Voucher = new VoucherDTO
                {
                    Code = carrinhoResponse.Voucher.Code,
                    Percentage = (decimal?)carrinhoResponse.Voucher.Percentage,
                    Discount = (decimal?)carrinhoResponse.Voucher.Discount,
                    DiscountType = carrinhoResponse.Voucher.Discounttype
                };
            }

            foreach (var item in carrinhoResponse.Items)
            {
                carrinhoDTO.Items.Add(new ShoppingCartItemDto
                {
                    Name = item.Name,
                    Image = item.Image,
                    ProductId = Guid.Parse(item.Productid),
                    Quantity = item.Quantity,
                    Price = (decimal)item.Price
                });
            }

            return carrinhoDTO;
        }
    }
}