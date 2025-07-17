using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BudgetPlus.Data;
using BudgetPlus.Models;
using BudgetPlus.Filters;

namespace BudgetPlus.Controllers
{
    [ServiceFilter(typeof(AuthFilter))]
    public class SharesController : Controller
    {
        private readonly AppDbContext _context;

        public SharesController(AppDbContext context)
        {
            _context = context;
        }


        // list of shares to pay by logged user
        [HttpGet]
        public async Task<IActionResult> ToPay()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var unPaidShares = await _context.Shares
                .Include(s => s.Expense)
                    .ThenInclude(e => e.User)
                .Include(s => s.Expense)
                    .ThenInclude(e => e.Category)
                .Where(s => s.OwedUserId == userId && !s.isPaid)
                .ToListAsync();
            return View(unPaidShares);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Auth", "Login");
            }

            var share = await _context.Shares
                .FirstOrDefaultAsync(s => s.Id == id && s.OwedUserId == userId.Value);

            if (share == null)
            {
                return NotFound();
            }

            // pay the share
            share.isPaid = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("ToPay");
        }

        [HttpGet]
        public async Task<IActionResult> MyExpenses()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var paidShares = await _context.Shares
                .Include(s => s.Expense)
                    .ThenInclude(e => e.User)
                .Include(s => s.Expense)
                    .ThenInclude(s => s.Category)
                .Where(s => s.OwedUserId == userId && s.isPaid)
                .ToListAsync();

            ViewBag.TotalExpenses = paidShares.Sum(s => s.Amount);

            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var monthlyExpenses = paidShares
                .Where(s => s.Expense.Date.Month == currentMonth && s.Expense.Date.Year == currentYear).ToList();
            ViewBag.MonthlyExpenses = monthlyExpenses.Sum(s => s.Amount);
            
            return View(paidShares);
        }


        private bool ShareExists(int id)
        {
            return _context.Shares.Any(e => e.Id == id);
        }
    }
}
