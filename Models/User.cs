using System.ComponentModel.DataAnnotations;

namespace BudgetPlus.Models;

public class User
{
	[Key]
	public int Id { get; set; }

	[Required]
	[MaxLength(50)]
	public string Username { get; set; } = string.Empty;

	[Required]
	public string PasswordHash { get; set; } = string.Empty;

	[Required]
	public string ApiToken { get; set; } = Guid.NewGuid().ToString();

	public ICollection<Expense> Expenses { get; set; } = new List<Expense>(); 
	public ICollection<Friend> Friends { get; set; } = new List<Friend>(); 
	

}