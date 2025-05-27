using RewardAPI.Data;
using Microsoft.EntityFrameworkCore;
using RewardAPI.Message;
using RewardAPI.Models;

namespace RewardAPI.Services;

public class RewardService : IRewardService
{
	private DbContextOptions<AppDbContext> _dbOptions;

	public RewardService(DbContextOptions<AppDbContext> dbOptions)
	{
		_dbOptions = dbOptions;
	}

	public async Task UpdateRewards(RewardsMessage rewardsMessage)
	{
		try
		{
			Rewards rewards = new Rewards()
			{
				OrderId = rewardsMessage.OrderId,
				RewardsActivity = rewardsMessage.RewardsActivity,
				UsedId = rewardsMessage.UserId,
				RewardsDate = DateTime.Now
			};

			await using var _db = new AppDbContext(_dbOptions);
			await _db.Rewards.AddAsync(rewards);
			await _db.SaveChangesAsync();
		}
		catch (Exception ex)
		{
		}
	}
}
