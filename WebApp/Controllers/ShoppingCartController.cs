using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using WebApp.Models;
using WebApp.Service.IService;

namespace WebApp.Controllers
{
	public class ShoppingCartController : Controller
	{
		private readonly IShoppingCartService _shoppingCartService;

		public ShoppingCartController(IShoppingCartService shoppingCartService)
		{
			_shoppingCartService = shoppingCartService;
		}

		[Authorize]
		public async Task<IActionResult> CartIndex()
		{
			return View(await LoadCartDtoBasedOnLoggedInUser());
		}

        public async Task<IActionResult> Remove(int cartDetailsId)
        {
            var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
            ResponseDto? response = await _shoppingCartService.RemoveFromCartAsync(cartDetailsId);

            if (response != null & response.IsSuccess)
            {
				TempData["success"] = "Cart updated successfully!";
				return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

		[HttpPost]
        public async Task<IActionResult> ApplyCoupon(CartDto cartDto)
        {
            ResponseDto? response = await _shoppingCartService.ApplyCouponAsync(cartDto);

            if (response != null & response.IsSuccess)
            {
                TempData["success"] = "Cart updated successfully!";
                return RedirectToAction(nameof(CartIndex));
            }
            return View();
        }

		[HttpPost]
		public async Task<IActionResult> EmailCart(CartDto cartDto)
		{
			ResponseDto? response = await _shoppingCartService.EmailCart(cartDto);

			if (response != null & response.IsSuccess)
			{
				TempData["success"] = "Email will be processed and sent shortly.";
				return RedirectToAction(nameof(CartIndex));
			}
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> RemoveCoupon(CartDto cartDto)
		{
			cartDto.CartHeader.CouponCode = "";
			ResponseDto? response = await _shoppingCartService.ApplyCouponAsync(cartDto);

			if (response != null & response.IsSuccess)
			{
				TempData["success"] = "Cart updated successfully!";
				return RedirectToAction(nameof(CartIndex));
			}
			return View();
		}

		private async Task<CartDto> LoadCartDtoBasedOnLoggedInUser()
		{
			var userId = User.Claims.Where(u => u.Type == JwtRegisteredClaimNames.Sub)?.FirstOrDefault()?.Value;
			ResponseDto? response = await _shoppingCartService.GetCartByUserIdAsync(userId);

			if (response != null & response.IsSuccess)
			{
				CartDto cartDto = JsonConvert.DeserializeObject<CartDto>(Convert.ToString(response.Result));
				return cartDto;
			}
			return new CartDto();
		}
	}
}
