using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Models;
using WebApp.Service.IService;
using WebApp.Utility;

namespace WebApp.Controllers
{
	public class AuthController : Controller
	{
		private IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpGet]

		public IActionResult Login()
		{
			LoginRequestDto loginRequestDto = new LoginRequestDto();
			return View(loginRequestDto);
		}

		[HttpGet]

		public IActionResult Register()
		{
			var roleList = new List<SelectListItem>()
			{
				new SelectListItem(SD.RoleAdmin, SD.RoleAdmin),
				new SelectListItem(SD.RoleCustomer, SD.RoleCustomer)
			};

			ViewBag.RoleList = roleList;
			return View();
		}

		public IActionResult Logout()
		{
			return View();
		}

	}
}
