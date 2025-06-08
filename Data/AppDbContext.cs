using Microsoft.EntityFrameworkCore;
using BudgetPlus.Models;

namespace BudgetPlus.Data;

public class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
	public DbSet<User> Users { get; set; }
	public DbSet<Expense> Expenses { get; set; }
	public DbSet<Friend> Freinds { get; set; }
	public DbSet<Category> Categories { get; set; }
	public DbSet<Share> Shares { get; set; }
	
	public DbSet<FriendRequest> FriendRequests { get; set;}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// Additional configuration of relation USER -> FRIEND
		modelBuilder.Entity<Friend>()
			.HasOne(f => f.User)
			.WithMany(u => u.Friends)
			.HasForeignKey(f => f.UserId)
			.OnDelete(DeleteBehavior.Cascade);


		modelBuilder.Entity<Friend>()
			.HasOne(f => f.FriendUser)
			.WithMany()
			.HasForeignKey(f => f.FriendUserId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}