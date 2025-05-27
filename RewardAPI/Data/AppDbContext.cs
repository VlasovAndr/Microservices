using Microsoft.EntityFrameworkCore;
using RewardAPI.Models;

namespace RewardAPI.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		{
		}

		public DbSet<Rewards> Rewards { get; set; }
	}
}
