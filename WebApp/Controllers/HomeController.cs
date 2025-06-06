using Duende.IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using WebApp.Models;
using WebApp.Service.IService;

namespace WebApp.Controllers
{
	public class HomeController : Controller
	{
		private readonly IProductService _productService;
		private readonly IShoppingCartService _shoppingCartService;

		public HomeController(IProductService productService, IShoppingCartService shoppingCartService)
		{
			_productService = productService;
			_shoppingCartService = shoppingCartService;
		}

		public async Task<IActionResult> Index()
		{
			List<ProductDto>? list = new List<ProductDto>();

			ResponseDto? response = await _productService.GetAllProductsAsync();

			if (response != null && response.IsSuccess)
			{
				list = JsonConvert.DeserializeObject<List<ProductDto>>(Convert.ToString(response.Result));
			}
			else
			{
				TempData["error"] = response?.Message;
			}

			return View(list);
		}

		[Authorize]
		public async Task<IActionResult> ProductDetails(int productId)
		{
			var product = new ProductDto();

			ResponseDto? response = await _productService.GetProductByIdAsync(productId);

			if (response != null && response.IsSuccess)
			{
				product = JsonConvert.DeserializeObject<ProductDto>(Convert.ToString(response.Result));
			}
			else
			{
				TempData["error"] = response?.Message;
			}

			return View(product);
		}

		[Authorize]
		[HttpPost]
		[ActionName("ProductDetails")]
		public async Task<IActionResult> ProductDetails(ProductDto productDto)
		{
			CartDto cartDto = new CartDto()
			{
				CartHeader = new CartHeaderDto
				{
					UserId = User.Claims.Where(u => u.Type == JwtClaimTypes.Subject)?.FirstOrDefault()?.Value
				}
			};

			CartDetailsDto cartDetailsDto = new CartDetailsDto()
			{
				Count = productDto.Count,
				ProductId = productDto.ProductId
			};

			List<CartDetailsDto> cartDetailsDtos = new List<CartDetailsDto>() { cartDetailsDto };
			cartDto.CartDetails = cartDetailsDtos;

			ResponseDto? response = await _shoppingCartService.UpsertCartAsync(cartDto);

			if (response != null && response.IsSuccess)
			{
				TempData["success"] = "Item has been added to the Shopping cart";
				return RedirectToAction(nameof(Index));
			}
			else
			{
				TempData["error"] = response?.Message;
			}

			return View(productDto);
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
