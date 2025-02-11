namespace AuthAPI.Models.Dto
{
	public class LoginResponseDto
	{
        public UserDto Usser { get; set; }
        public string Token { get; set; }
	}
}
