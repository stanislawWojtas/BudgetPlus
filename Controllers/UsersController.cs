using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BudgetPlus.Data;
using BudgetPlus.Models;
using Microsoft.AspNetCore.Authorization;
using BudgetPlus.Filters;

namespace BudgetPlus.Controllers
{
    [ServiceFilter(typeof(AdminFilter))]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Delete related Friend records where user is either the owner or friend
                var friendships = _context.Freinds.Where(f => f.UserId == id || f.FriendUserId == id);
                _context.Freinds.RemoveRange(friendships);

                // Delete related Share records
                var shares = _context.Shares.Where(s => s.OwedUserId == id);
                _context.Shares.RemoveRange(shares);

                // Delete related Expense records
                var expenses = _context.Expenses.Where(e => e.UserId == id);
                _context.Expenses.RemoveRange(expenses);

                // Finally delete the user
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
