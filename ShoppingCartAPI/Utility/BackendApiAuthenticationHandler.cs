
using Microsoft.AspNetCore.Authentication;

namespace ShoppingCartAPI.Utility
{
	public class BackendApiAuthenticationHandler : DelegatingHandler
	{
		private readonly IHttpContextAccessor _accessor;

		public BackendApiAuthenticationHandler(IHttpContextAccessor accessor)
		{
			_accessor = accessor;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var token = await _accessor.HttpContext.GetTokenAsync("access_token");
			request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

			return await base.SendAsync(request, cancellationToken);
		}
	}
}
