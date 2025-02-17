using AuthAPI.Models.Dto;
using AuthAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace AuthAPI.Controllers
{
	[Route("api/auth")]
	[ApiController]
	public class AuthAPIController : ControllerBase
	{
		private readonly  IAuthService _authService;
		protected ResponseDto _responce;

		public AuthAPIController(IAuthService authService)
		{
			_authService = authService;
			_responce = new ResponseDto();
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegistrationRequestDto model)
		{
			var errorMessage = await _authService.Register(model);

			if (!string.IsNullOrEmpty(errorMessage)) 
			{
				_responce.IsSuccess = false;
				_responce.Message = errorMessage;
				return BadRequest(_responce);
			}

			return Ok(_responce);
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login()
		{
			return Ok();
		}

	}
}
