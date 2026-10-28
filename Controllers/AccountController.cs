using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AuditFlow.Web.Data;
using AuditFlow.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuditFlow.Web.Controllers;

public class AccountController(AuditFlowDbContext db, IPasswordHasher<AppUser> hasher) : Controller
{
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) => User.Identity?.IsAuthenticated == true ? RedirectToAction("Index", "Home") : View(new LoginViewModel { ReturnUrl = returnUrl });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == model.UserName.Trim(), ct);
        if (user is null || !user.IsActive || hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Invalid username or password, or the account is inactive.");
            return View(model);
        }
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, user.Id.ToString()), new(ClaimTypes.Name, user.DisplayName), new(ClaimTypes.Role, user.Role), new("activeRole", user.Role), new("username", user.UserName) };
        var assignedRoles = await db.UserProjectAssignments.Where(a => a.UserId == user.Id && a.IsActive).Select(a => a.Role).Distinct().ToListAsync(ct);
        foreach (var assignedRole in assignedRoles.Where(r => !string.Equals(r, user.Role, StringComparison.Ordinal))) claims.Add(new Claim("availableRole", assignedRole));
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });
        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl)) return LocalRedirect(model.ReturnUrl);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost, Authorize, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> Roles(CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var current = User.FindFirstValue("activeRole") ?? User.FindFirstValue(ClaimTypes.Role) ?? "";
        var roles = await db.UserProjectAssignments.Where(a => a.UserId == userId && a.IsActive).Select(a => a.Role).Distinct().ToListAsync(ct);
        var userRole = await db.Users.Where(u => u.Id == userId).Select(u => u.Role).FirstOrDefaultAsync(ct);
        if (!string.IsNullOrWhiteSpace(userRole) && !roles.Contains(userRole)) roles.Add(userRole);
        ViewBag.CurrentRole = current;
        ViewBag.Roles = roles.Select(r => new RoleChoice(r, AppRoles.Label(r))).OrderBy(r => r.Label).ToList();
        return View();
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SwitchRole(string role, CancellationToken ct)
    {
        if (!AppRoles.All.Contains(role)) return Forbid();
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var primary = await db.Users.Where(u => u.Id == userId).Select(u => u.Role).FirstOrDefaultAsync(ct);
        var assigned = await db.UserProjectAssignments.AnyAsync(a => a.UserId == userId && a.Role == role && a.IsActive, ct);
        if (role != primary && !assigned) return Forbid();
        var claims = User.Claims.Where(c => c.Type != ClaimTypes.Role && c.Type != "activeRole" && c.Type != "availableRole").ToList();
        claims.Add(new Claim(ClaimTypes.Role, role)); claims.Add(new Claim("activeRole", role));
        var allRoles = await db.UserProjectAssignments.Where(a => a.UserId == userId && a.IsActive).Select(a => a.Role).Distinct().ToListAsync(ct);
        foreach (var r in allRoles) if (r != role) claims.Add(new Claim("availableRole", r));
        if (!string.IsNullOrWhiteSpace(primary) && primary != role && !allRoles.Contains(primary)) claims.Add(new Claim("availableRole", primary));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new System.Security.Claims.ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)), new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });
        TempData["Success"] = $"Switched view to {AppRoles.Label(role)}.";
        return RedirectToAction("Index", "Home");
    }


    [Authorize, HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, ct);
        if (user is null) return Challenge();
        if (hasher.VerifyHashedPassword(user, user.PasswordHash, model.CurrentPassword) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), "The current password is incorrect.");
            return View(model);
        }
        user.PasswordHash = hasher.HashPassword(user, model.NewPassword);
        await db.SaveChangesAsync(ct);
        TempData["Success"] = "Password changed successfully.";
        return RedirectToAction(nameof(Roles));
    }

    [HttpGet] public IActionResult Denied() => View();
}
