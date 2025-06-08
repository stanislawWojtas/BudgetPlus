using BudgetPlus.Models;
namespace BudgetPlus.ViewModels
{
	public class FriendBalanceViewModel
	{
		public User Friend { get; set; } = null!;
		public decimal Balance{ get; set; }
	}
}