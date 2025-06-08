using System.Security.Cryptography;
using System.Text;
using BudgetPlus.Data;
using BudgetPlus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("auth")]
public class AuthController : Controller
{
	private readonly AppDbContext _context;

	public AuthController(AppDbContext context)
	{
		_context = context;
	}

	[HttpGet("register")]
	[AllowAnonymous]
	public IActionResult Register() => View();

	[HttpPost("register")]
	[AllowAnonymous]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Register(string username, string password)
	{
		if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
		{
			ViewBag.ErrorMessage = "Username and password cannot be empty.";
			return View();
		}

		if (_context.Users.Any(u => u.Username == username))
		{
			ViewBag.ErrorMessage = "User already exists.";
			return View();
		}

		var user = new User
		{
			Username = username,
			PasswordHash = HashPassword(password),
			ApiToken = Guid.NewGuid().ToString()
		};

		_context.Users.Add(user);
		await _context.SaveChangesAsync();
		ViewBag.SuccessMessage = "Registration successful! You can now log in.";
		return await Login(username, password);
	}

	[HttpGet("login")]
	[AllowAnonymous]
	public IActionResult Login() => View();

	[HttpPost("login")]
	[AllowAnonymous]
	public async Task<IActionResult> Login(string username, string password)
	{
		if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
		{
			ViewBag.ErrorMessage = "Username and password cannot be empty.";
			return View();
		}

		var hash = HashPassword(password);
		var user = _context.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == hash);

		if (user == null)
		{
			ViewBag.ErrorMessage = "Invalid username or password.";
			return View();
		}

		// Save data to session
		HttpContext.Session.SetInt32("UserId", user.Id);
		HttpContext.Session.SetString("Username", user.Username);
		HttpContext.Session.SetString("logged", "true");
		HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString().ToLower());

		return RedirectToAction("Index", "Home");
	}

	[HttpGet("logout")]
	public IActionResult Logout()
	{
		HttpContext.Session.Clear();
		TempData["LogoutMessage"] = "Are you sure you want to log out?";
		return RedirectToAction("Login");
	}

	public string HashPassword(string password)
	{
		using var md5 = MD5.Create();
		byte[] inputBytes = Encoding.ASCII.GetBytes(password);
		byte[] hashBytes = md5.ComputeHash(inputBytes);
		return Convert.ToHexString(hashBytes);
	}
}