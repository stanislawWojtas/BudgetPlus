using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BudgetPlus.Data;
using BudgetPlus.Models;
using BudgetPlus.ViewModels;
using BudgetPlus.Filters;

namespace BudgetPlus.Controllers
{
    [ServiceFilter(typeof(AuthFilter))]
    public class FriendsController : Controller
    {
        private readonly AppDbContext _context;

        public FriendsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Friends
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var friends = await _context.Freinds
                .Include(f => f.FriendUser)
                .Where(f => f.UserId == userId)
                .Select(f => new FriendBalanceViewModel
                {
                    Friend = f.FriendUser!,
                    Balance = _context.Shares
                        .Where(s => !s.isPaid && 
                            ((s.OwedUserId == userId && s.Expense!.UserId == f.FriendUserId) ||
                             (s.OwedUserId == f.FriendUserId && s.Expense!.UserId == userId)))
                        .Sum(s => s.OwedUserId == userId ? -s.Amount : s.Amount)
                })
                .ToListAsync();

            return View(friends);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SettleUp(int friendId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            //Start the transaction (all operation must be done to be saved)
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Set all unpaid shares between those users to paid ones
                var sharesToSettle = await _context.Shares
                    .Where(s => !s.isPaid &&
                    ((s.OwedUserId == userId && s.Expense!.UserId == friendId) ||
                    (s.OwedUserId == friendId && s.Expense!.UserId == userId)))
                    .ToListAsync();

                foreach (var share in sharesToSettle)
                {
                    share.isPaid = true;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                return RedirectToAction("Index");
            }
        }

        // GET: Friends/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var friend = await _context.Freinds
                .Include(f => f.FriendUser)
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (friend == null)
            {
                return NotFound();
            }

            return View(friend);
        }

        // POST: Friends/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var friend = await _context.Freinds
                .Include(f => f.FriendUser)
                .FirstOrDefaultAsync(f => f.Id == id);
            if (friend != null)
            {
                //remove both Friend relations
                _context.Freinds.Remove(friend);

                var reverseFriend = await _context.Freinds
                    .FirstOrDefaultAsync(f => f.UserId == friend.FriendUserId && f.FriendUserId == friend.UserId);

                if (reverseFriend != null)
                {
                    _context.Freinds.Remove(reverseFriend);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult SendRequest() => View();

        [HttpPost]
        public async Task<IActionResult> SendRequest(string username)
        {
            var senderId = HttpContext.Session.GetInt32("UserId");
            if (senderId == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (receiver == null)
            {
                ModelState.AddModelError("", "User not found");
                return View();
            }
            if (username == HttpContext.Session.GetString("Username"))
            {
                ModelState.AddModelError("", "Cannot send request to yourself");
                return View();
            }

            // checking if request already exist
            var existingRequest = await _context.FriendRequests.AnyAsync(fr => (fr.SenderId == senderId && fr.ReceiverId == receiver.Id && fr.Status == "Pending") ||
                                                                                (fr.SenderId == receiver.Id && fr.ReceiverId == senderId && fr.Status == "Pending"));
            if (existingRequest)
            {
                ModelState.AddModelError("", "Friend request already exist");
                return View();
            }

            // now we create the request
            var friendRequest = new FriendRequest
            {
                SenderId = senderId.Value,
                ReceiverId = receiver.Id
            };
            _context.FriendRequests.Add(friendRequest);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // active requests for User
        [HttpGet]
        public async Task<IActionResult> Requests()
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var requests = await _context.FriendRequests
                .Include(fr => fr.Sender)
                .Where(fr => fr.ReceiverId == currentUserId && fr.Status == "Pending")
                .ToListAsync();
            return View(requests);
        }

        // methon
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RespondToRequest(int requestId, bool accept)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Auth");
            }
            var request = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.ReceiverId == currentUserId && requestId == fr.Id);

            if (request == null)
            {
                return NotFound();
            }

            // adding users as friends
            if (accept)
            {
                var friendship1 = new Friend
                {
                    UserId = request.SenderId,
                    FriendUserId = request.ReceiverId
                };
                var friendship2 = new Friend
                {
                    UserId = request.ReceiverId,
                    FriendUserId = request.SenderId
                };
                _context.Freinds.Add(friendship1);
                _context.Freinds.Add(friendship2);
                request.Status = "Accepted";
            }
            else
            {
                request.Status = "Rejected";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}