using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FileVaultAdmin.Helpers;
using FileVaultAdmin.Models.ViewModels;
using FileVaultAdmin.Services;

namespace FileVaultAdmin.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly AdminService _svc;

    public UsersController(AdminService svc) => _svc = svc;

    public async Task<IActionResult> Index(int page = 1, string? search = null,
        string? role = null, string? status = null)
    {
        var pageSize = 25;
        var users = await _svc.GetUsersAsync(page, pageSize, search, role, status);
        var total = await _svc.CountUsersAsync(search, role, status);

        var model = new UserListViewModel
        {
            Users = users.Select(u => new UserRowViewModel
            {
                Id = u.Id, FullName = u.FullName, Email = u.Email,
                Roles = u.Roles, IsActive = u.IsActive, EmailConfirmed = u.EmailConfirmed,
                StorageUsedBytes = u.StorageUsedBytes,
                StorageFormatted = FormatHelpers.FormatFileSize(u.StorageUsedBytes),
                CreatedAt = u.CreatedAt
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalUsers = total,
            SearchTerm = search,
            RoleFilter = role,
            StatusFilter = status
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Disable(string id)
    {
        await _svc.DisableUserAsync(id);
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var adminEmail = User.FindFirstValue(ClaimTypes.Email);
        await _svc.LogActionAsync(adminId, adminEmail, "UserDisabled", "User", id,
            HttpContext.Connection.RemoteIpAddress?.ToString());
        TempData["Success"] = "User disabled.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Enable(string id)
    {
        await _svc.EnableUserAsync(id);
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var adminEmail = User.FindFirstValue(ClaimTypes.Email);
        await _svc.LogActionAsync(adminId, adminEmail, "UserEnabled", "User", id,
            HttpContext.Connection.RemoteIpAddress?.ToString());
        TempData["Success"] = "User enabled.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleAdmin(string id)
    {
        await _svc.ToggleRoleAsync(id, "Admin");
        TempData["Success"] = "User role updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Create(string fullName, string email, string password, bool isAdmin)
    {
        if (await _svc.GetUserByEmailAsync(email) != null)
        {
            TempData["Error"] = "User with this email already exists.";
            return RedirectToAction(nameof(Index));
        }

        var user = new FileVaultAdmin.Models.Domain.AppUser
        {
            FullName = fullName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Roles = new List<string> { "User" },
            IsActive = true,
            EmailConfirmed = true
        };

        if (isAdmin) user.Roles.Add("Admin");

        await _svc.CreateUserAsync(user);
        
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var adminEmail = User.FindFirstValue(ClaimTypes.Email);
        await _svc.LogActionAsync(adminId, adminEmail, "UserCreated", "User", user.Id, 
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = "User created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Update(string id, string fullName, string email, string? password, bool isAdmin)
    {
        await _svc.UpdateUserAsync(id, fullName, email, password, isAdmin);
        
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var adminEmail = User.FindFirstValue(ClaimTypes.Email);
        await _svc.LogActionAsync(adminId, adminEmail, "UserUpdated", "User", id, 
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = "User details updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetDetails(string id)
    {
        var user = await _svc.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return Json(new { user.Id, user.FullName, user.Email, user.IsActive, isAdmin = user.Roles.Contains("Admin") });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (id == currentUserId)
        {
            TempData["Error"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Index));
        }

        await _svc.DeleteUserAsync(id);
        var adminEmail = User.FindFirstValue(ClaimTypes.Email);
        await _svc.LogActionAsync(currentUserId, adminEmail, "UserDeleted", "User", id,
            HttpContext.Connection.RemoteIpAddress?.ToString());
        TempData["Success"] = "User deleted.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(string id)
    {
        var user = await _svc.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        var fileCount = await _svc.GetUserFilesCountAsync(id);
        var folderCount = await _svc.GetUserFoldersCountAsync(id);
        var recentLogs = await _svc.GetUserRecentActivityAsync(id, 15);

        var model = new UserDetailsViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Roles = user.Roles,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            StorageUsedBytes = user.StorageUsedBytes,
            StorageFormatted = FormatHelpers.FormatFileSize(user.StorageUsedBytes),
            CreatedAt = user.CreatedAt,
            PasswordHash = user.PasswordHash,
            FileCount = fileCount,
            FolderCount = folderCount,
            RecentActivity = recentLogs.Select(l => new AuditLogRowViewModel
            {
                Id = l.Id,
                Action = l.Action,
                TargetType = l.TargetType,
                TargetId = l.TargetId,
                IpAddress = l.IpAddress,
                CreatedAt = l.CreatedAt
            }).ToList()
        };

        return View(model);
    }
}
