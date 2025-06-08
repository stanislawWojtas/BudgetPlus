using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace BudgetPlus.Models;

public class Share
{
	[Key]
	public int Id { get; set; }

	[Required]
	public int ExpenseId { get; set; }

	[Required]
	public int OwedUserId { get; set; }

	[Required]
	[Range(0.01, double.MaxValue, ErrorMessage ="Amount must me larger than 0")]
	public decimal Amount { get; set; }

	public bool isPaid { get; set; } = false;

	[ForeignKey("ExpenseId")]
	[DeleteBehavior(DeleteBehavior.Cascade)]
	public Expense? Expense { get; set; }

	[ForeignKey("OwedUserId")]
	public User? OwedUser{ get; set; }
}