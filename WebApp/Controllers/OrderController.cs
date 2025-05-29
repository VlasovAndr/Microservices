using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using WebApp.Models;
using WebApp.Service.IService;
using WebApp.Utility;

namespace WebApp.Controllers
{
	public class OrderController : Controller
	{
		private readonly IOrderService _orderService;
		public OrderController(IOrderService orderService)
		{
			_orderService = orderService;
		}

		public IActionResult OrderIndex()
		{
			return View();
		}

		[HttpGet]
		public IActionResult GetAll()
		{
			IEnumerable<OrderHeaderDto> list;
			string userId = "";
			if (!User.IsInRole(SD.RoleAdmin))
			{
				userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
			}
			ResponseDto response = _orderService.GetAllOrders(userId).GetAwaiter().GetResult();
			if (response != null && response.IsSuccess)
			{
				list = JsonConvert.DeserializeObject<List<OrderHeaderDto>>(Convert.ToString(response.Result));
			}
			else
			{
				list = new List<OrderHeaderDto>();
			}
			return Json(new { data = list.OrderByDescending(u => u.OrderHeaderId) });
		}
	}
}
