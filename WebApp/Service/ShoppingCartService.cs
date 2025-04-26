using WebApp.Models;
using WebApp.Service.IService;
using WebApp.Utility;

namespace WebApp.Service
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IBaseService _baseService;

        public ShoppingCartService(IBaseService baseService)
        {
            _baseService = baseService;
        }

        public async Task<ResponseDto?> ApplyCouponAsync(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Data = cartDto,
                Url = SD.ShoppingCartAPIBaseUrl + "/api/cart/ApplyCoupon"
            });
        }

		public async Task<ResponseDto?> EmailCart(CartDto cartDto)
		{
			return await _baseService.SendAsync(new RequestDto()
			{
				ApiType = SD.ApiType.POST,
				Data = cartDto,
				Url = SD.ShoppingCartAPIBaseUrl + "/api/cart/EmailCartRequest"
			});
		}

		public async Task<ResponseDto?> GetCartByUserIdAsync(string userId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.GET,
                Url = SD.ShoppingCartAPIBaseUrl + $"/api/cart/GetCart/{userId}"
            });
        }

        public async Task<ResponseDto?> RemoveFromCartAsync(int cartDetailsId)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Data = cartDetailsId,
                Url = SD.ShoppingCartAPIBaseUrl + "/api/cart/RemoveCart"
            });
        }

        public async Task<ResponseDto?> UpsertCartAsync(CartDto cartDto)
        {
            return await _baseService.SendAsync(new RequestDto()
            {
                ApiType = SD.ApiType.POST,
                Data = cartDto,
                Url = SD.ShoppingCartAPIBaseUrl + "/api/cart/CartUpsert"
            });
        }
    }
}
