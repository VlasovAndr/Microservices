using EmailAPI.Message;
using EmailAPI.Models.Dtos;

namespace EmailAPI.Services;

public interface IEmailService
{
	Task EmailCartAndLog(CartDto cartDto);
	Task RegisterUserEmailAndLog(string email);
	Task LogPlacedOrder(RewardsMessage rewardsMessage);
}
