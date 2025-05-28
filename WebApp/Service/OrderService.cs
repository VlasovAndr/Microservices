using WebApp.Models;
using WebApp.Service.IService;
using WebApp.Utility;

namespace WebApp.Service
{
	public class OrderService : IOrderService
	{
		private readonly IBaseService _baseService;

		public OrderService(IBaseService baseService)
		{
			_baseService = baseService;
		}

		public async Task<ResponseDto?> CreateOrder(CartDto cartDto)
		{
			return await _baseService.SendAsync(new RequestDto()
			{
				ApiType = SD.ApiType.POST,
				Data = cartDto,
				Url = SD.OrderAPIBaseUrl + "/api/order/CreateOrder"
			});
		}

		public async Task<ResponseDto?> CreateStripeSession(StripeRequestDto stripeRequestDto)
		{
			return await _baseService.SendAsync(new RequestDto()
			{
				ApiType = SD.ApiType.POST,
				Data = stripeRequestDto,
				Url = SD.OrderAPIBaseUrl + "/api/order/CreateStripeSession"
			});
		}

		public async Task<ResponseDto?> GetAllOrders(string? userId)
		{
			return await _baseService.SendAsync(new RequestDto()
			{
				ApiType = SD.ApiType.GET,
				Url = SD.OrderAPIBaseUrl + "/api/order/GetOrders/" + userId
			});
		}

		public async Task<ResponseDto?> GetOrder(int orderId)
		{
			return await _baseService.SendAsync(new RequestDto()
			{
				ApiType = SD.ApiType.GET,
				Url = SD.OrderAPIBaseUrl + "/api/order/GetOrder/" + orderId
			});
		}

		public async Task<ResponseDto?> UpdateOrderStatus(int orderId, string newStatus)
		{
			return await _baseService.SendAsync(new RequestDto()
			{
				ApiType = SD.ApiType.POST,
				Data = newStatus,
				Url = SD.OrderAPIBaseUrl + "/api/order/UpdateOrderStatus/" + orderId
			});
		}

		public async Task<ResponseDto?> ValidateStripeSession(int orderHeaderId)
		{
			return await _baseService.SendAsync(new RequestDto()
			{
				ApiType = SD.ApiType.POST,
				Data = orderHeaderId,
				Url = SD.OrderAPIBaseUrl + "/api/order/ValidateStripeSession"
			});
		}
	}
}
