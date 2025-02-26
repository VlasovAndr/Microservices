using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using WebApp.Service.IService;

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
			return View();
		}

		public IActionResult Logout()
		{
			return View();
		}

	}
}
