﻿using AuthAPI.Models.Dto;
using AuthAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace AuthAPI.Controllers
{
	[Route("api/auth")]
	[ApiController]
	public class AuthAPIController : ControllerBase
	{
		private readonly IAuthService _authService;
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
		public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
		{
			var loginResponse = await _authService.Login(model);

			if (loginResponse.User == null)
			{
				_responce.IsSuccess = false;
				_responce.Message = "Username or password is incorrect";
				return BadRequest(_responce);

			}
			_responce.Result = loginResponse;
			return Ok(_responce);
		}

		[HttpPost("AssignRole")]
		public async Task<IActionResult> AssignRole([FromBody] RegistrationRequestDto model)
		{
			var assignRoleSuccessful = await _authService.AssignRole(model.Email, model.Role.ToUpper());

			if (!assignRoleSuccessful)
			{
				_responce.IsSuccess = false;
				_responce.Message = "Error encountred";
				return BadRequest(_responce);

			}

			return Ok(_responce);
		}

	}
}
