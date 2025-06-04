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

namespace BudgetPlus.Controllers
{
    [Authorize]
    public class SharesController : Controller
    {
        private readonly AppDbContext _context;

        public SharesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Shares
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Shares.Include(s => s.Expense).Include(s => s.OwedUser);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Shares/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var share = await _context.Shares
                .Include(s => s.Expense)
                .Include(s => s.OwedUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (share == null)
            {
                return NotFound();
            }

            return View(share);
        }

        // GET: Shares/Create
        public IActionResult Create()
        {
            ViewData["ExpenseId"] = new SelectList(_context.Expenses, "Id", "Description");
            ViewData["OwedUserId"] = new SelectList(_context.Users, "Id", "Username");
            return View();
        }

        // POST: Shares/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ExpenseId,OwedUserId,Amount,isPaid")] Share share)
        {
            if (ModelState.IsValid)
            {
                _context.Add(share);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ExpenseId"] = new SelectList(_context.Expenses, "Id", "Description", share.ExpenseId);
            ViewData["OwedUserId"] = new SelectList(_context.Users, "Id", "Username", share.OwedUserId);
            return View(share);
        }

        // GET: Shares/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var share = await _context.Shares.FindAsync(id);
            if (share == null)
            {
                return NotFound();
            }
            ViewData["ExpenseId"] = new SelectList(_context.Expenses, "Id", "Description", share.ExpenseId);
            ViewData["OwedUserId"] = new SelectList(_context.Users, "Id", "Username", share.OwedUserId);
            return View(share);
        }

        // POST: Shares/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ExpenseId,OwedUserId,Amount,isPaid")] Share share)
        {
            if (id != share.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(share);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShareExists(share.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ExpenseId"] = new SelectList(_context.Expenses, "Id", "Description", share.ExpenseId);
            ViewData["OwedUserId"] = new SelectList(_context.Users, "Id", "Username", share.OwedUserId);
            return View(share);
        }

        // GET: Shares/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var share = await _context.Shares
                .Include(s => s.Expense)
                .Include(s => s.OwedUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (share == null)
            {
                return NotFound();
            }

            return View(share);
        }

        // POST: Shares/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var share = await _context.Shares.FindAsync(id);
            if (share != null)
            {
                _context.Shares.Remove(share);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ShareExists(int id)
        {
            return _context.Shares.Any(e => e.Id == id);
        }
    }
}
