using EmailAPI.Models.Dtos;

namespace EmailAPI.Services;

public interface IEmailService
{
	Task EmailCartAndLog(CartDto cartDto);
}
