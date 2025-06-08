using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BudgetPlus.Data;
using BudgetPlus.Models;
using BudgetPlus.Filters;


namespace BudgetPlus.Controllers
{
    [ServiceFilter(typeof(AuthFilter))]
    public class ExpensesController : Controller
    {
        private readonly AppDbContext _context;

        public ExpensesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Expenses/Details/5
        // GET: Expenses/Create
        [HttpGet]
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Share can be added only to friends
            var friends = _context.Freinds
                .Where(f => f.UserId == userId.Value)
                .Select(f => f.FriendUser)
                .ToList();

            ViewBag.UserId = userId.Value;
            ViewBag.Friends = friends; //friends list to View
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Expenses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Amount,CategoryId,Description")] Expense expense, List<int> sharedUserIds)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrEmpty(expense.Description))
            {
                ModelState.AddModelError("Description", "Description is required");
            }

            if (sharedUserIds == null || !sharedUserIds.Any())
            {
                ModelState.AddModelError("sharedUserIds", "At least one participant must be selected");
                PrepareViewData(userId.Value, expense);
                return View(expense);
            }

            // Validate if all users exist
            var validUsers = await _context.Users
                .Where(u => sharedUserIds.Contains(u.Id))
                .Select(u => u.Id)
                .ToListAsync();

            if (validUsers.Count != sharedUserIds.Count)
            {
                ModelState.AddModelError("", "Invalid user selection");
                PrepareViewData(userId.Value, expense);
                return View(expense);
            }

            expense.UserId = userId.Value;
            expense.Date = DateTime.Now;

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Expenses.Add(expense);
                    await _context.SaveChangesAsync();

                    decimal sharedAmount = expense.Amount / sharedUserIds.Count;

                    var shares = sharedUserIds.Select(participantId => new Share
                    {
                        ExpenseId = expense.Id,
                        OwedUserId = participantId,
                        Amount = sharedAmount,
                        isPaid = participantId == userId.Value
                    }).ToList();

                    _context.Shares.AddRange(shares);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return RedirectToAction("Details", new {id = expense.Id});
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Error occurred while saving the expense: " + ex.Message);
                    PrepareViewData(userId.Value, expense);
                    return View(expense);
                }
            }

            PrepareViewData(userId.Value, expense);
            return View(expense);
        }

        // GET: Expenses/Delete/5
            [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var expense = await _context.Expenses
                .Include(e => e.Category)
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        // POST: Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                _context.Expenses.Remove(expense);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("MyExpenses", "Shares");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var expense = await _context.Expenses
                .Include(e => e.Category)
                .Include(e => e.User)
                .Include(e => e.Shares) // share for this expense
                .ThenInclude(s => s.OwedUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (expense == null || expense.UserId != HttpContext.Session.GetInt32("UserId"))
            {
                return NotFound();
            }
            return View(expense);
        }

        private void PrepareViewData(int userId, Expense expense)
        {
            ViewBag.Friends = _context.Freinds
                .Where(f => f.UserId == userId)
                .Select(f => f.FriendUser)
                .ToList();
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", expense.CategoryId);
            ViewBag.UserId = userId;
        }
    }
}
