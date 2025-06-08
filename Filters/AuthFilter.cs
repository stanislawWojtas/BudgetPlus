using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using BudgetPlus.Data;

namespace BudgetPlus.Filters;

public class AuthFilter : IAsyncActionFilter
{
    private readonly AppDbContext _context;

    public AuthFilter(AppDbContext context)
    {
        _context = context;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userId = context.HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        var hasPendingRequests = await _context.FriendRequests
            .AnyAsync(fr => fr.ReceiverId == userId && fr.Status == "Pending");
        
        context.HttpContext.Session.SetString("Request", hasPendingRequests.ToString().ToLower());
        
        await next();
    }
}

public class AdminFilter : IAsyncActionFilter
{
    private readonly AppDbContext _context;

    public AdminFilter(AppDbContext context)
    {
        _context = context;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userId = context.HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null || !user.IsAdmin)
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}