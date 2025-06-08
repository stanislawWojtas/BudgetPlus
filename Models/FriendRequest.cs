using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetPlus.Models;

public class FriendRequest
{
	[Key]
	public int Id { get; set; }

	[Required]
	public int SenderId { get; set; }

	[Required]
	public int ReceiverId { get; set; }

	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	public string Status { get; set; } = "Pending";// "pending", "accepted" or "rejected"

	[ForeignKey("SenderId")]
    public User? Sender { get; set; }

    [ForeignKey("ReceiverId")]
    public User? Receiver { get; set; }
	
}