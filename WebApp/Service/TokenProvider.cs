﻿using WebApp.Service.IService;
using WebApp.Utility;

namespace WebApp.Service
{
	public class TokenProvider : ITokenProvider
	{
		private readonly IHttpContextAccessor _contextAccessor;

		public TokenProvider(IHttpContextAccessor contextAccessor)
		{
			_contextAccessor = contextAccessor;
		}

		public void CleanToken()
		{
			_contextAccessor.HttpContext?.Response.Cookies.Delete(SD.TokenCookie);
		}

		public string? GetToken()
		{
			string? token = null;
			bool? hasToken = _contextAccessor.HttpContext?.Request.Cookies.TryGetValue(SD.TokenCookie, out token);
			return hasToken is true ? token : null;
		}

		public void SetToken(string token)
		{
			_contextAccessor.HttpContext?.Response.Cookies.Append(SD.TokenCookie, token);
		}
	}
}
