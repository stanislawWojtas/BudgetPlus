using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetPlus.Models;

public class Expense
{
	[Key]
	public int Id { get; set; }

	[Required]
	public int UserId { get; set; }

	[Required]
	[Range(0.01, double.MaxValue, ErrorMessage ="Amount must me larger than 0")]
	public decimal Amount { get; set; }

	[Required]
	public DateTime Date { get; set; }

	[Required]
	public int CategoryId { get; set; }

	[Required]
	[MaxLength(200)]
	public string Description { get; set; } = string.Empty;

	[ForeignKey("UserId")]
	public User? User { get; set; }

	[ForeignKey("CategoryId")]
	public Category? Category { get; set; }

	public ICollection<Share> Shares { get; set; } = new List<Share>();
	
}