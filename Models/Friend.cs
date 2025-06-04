using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetPlus.Models;

public class Friend
{
	[Key]
	public int Id { get; set; }

	[Required]
	public int UserId { get; set; }

	[Required]
	public int FriendUserId { get; set; }

	[ForeignKey("UserId")]
	public User? User { get; set; }

	[ForeignKey("FriendUserId")]
	public User? FriendUser{ get; set; }
}