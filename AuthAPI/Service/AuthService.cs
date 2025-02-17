using AuthAPI.Data;
using AuthAPI.Models;
using AuthAPI.Models.Dto;
using AuthAPI.Service.IService;
using Microsoft.AspNetCore.Identity;

namespace AuthAPI.Service
{
	public class AuthService : IAuthService
	{
		private readonly AppDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public AuthService(AppDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_db = db;
			_userManager = userManager;
			_roleManager = roleManager;
		}

		public Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto)
		{
			throw new NotImplementedException();
		}

		public async Task<string> Register(RegistrationRequestDto registrationRequestDto)
		{
			ApplicationUser user = new ApplicationUser()
			{
				UserName = registrationRequestDto.Name,
				Email = registrationRequestDto.Email,
				NormalizedEmail = registrationRequestDto.Email.ToUpper(),
				Name = registrationRequestDto.Name,
				PhoneNumber = registrationRequestDto.PhoneNumber
			};

			try
			{
				var result = await _userManager.CreateAsync(user, registrationRequestDto.Password);

				if (result.Succeeded)
				{
					var userToReturn = _db.ApplicationUsers.First(u => u.UserName == registrationRequestDto.Name);

					UserDto userDto = new UserDto()
					{
						Email = userToReturn.Email,
						ID = userToReturn.Id,
						Name = userToReturn.Name,
						PhoneNumber = userToReturn.PhoneNumber
					};

					return "";
				}
				else
				{
					return result.Errors.FirstOrDefault().Description;

				}
			}
			catch (Exception)
			{
			}

			return "Error Encountred!";
		}
	}
}
