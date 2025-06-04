using System.ComponentModel.DataAnnotations;

namespace BudgetPlus.Models;

public class Category
{
	[Key]
	public int Id { get; set; }

	[Required]
	[MaxLength(50)]
	public string Name { get; set; } = string.Empty;

	public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}