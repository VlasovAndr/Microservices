using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers
{
	public class OrderController : Controller
	{
		public IActionResult OrderIndex()
		{
			return View();
		}
	}
}
