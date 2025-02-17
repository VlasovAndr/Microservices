using AuthAPI.Models.Dto;

namespace AuthAPI.Service.IService
{
	public interface IAuthService
	{
		Task<UserDto> Register(RegistrationRequestDto registrationRequestDto);
		Task<LoginResponseDto> Login(LoginRequestDto loginRequestDto);
	}
}
