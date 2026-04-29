using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FileVaultAdmin.Models.ViewModels;
using FileVaultAdmin.Services;
using FileVaultAdmin.Helpers;

namespace FileVaultAdmin.Controllers;

[Authorize]
public class SharesController : Controller
{
    private readonly AdminService _svc;

    public SharesController(AdminService svc) => _svc = svc;

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        var pageSize = 50;
        var shares = await _svc.GetSharesAsync(page, pageSize, search);
        var total = await _svc.CountSharesAsync(search);

        // Resolve file names & owner emails
        var shareVMs = new List<ShareRowViewModel>();
        foreach (var s in shares)
        {
            var owner = await _svc.GetUserByIdAsync(s.OwnerUserId);
            shareVMs.Add(new ShareRowViewModel
            {
                Id = s.Id, FileId = s.FileId, Token = s.Token,
                OwnerUserId = s.OwnerUserId, OwnerEmail = owner?.Email,
                ExpiresAt = s.ExpiresAt, AllowDownload = s.AllowDownload,
                AccessCount = s.AccessCount, IsRevoked = s.IsRevoked, CreatedAt = s.CreatedAt
            });
        }

        var model = new ShareListViewModel
        {
            Shares = shareVMs, Page = page, PageSize = pageSize, TotalShares = total,
            SearchTerm = search
        };

        ViewBag.AllUsers = await _svc.GetAllUsersBasicAsync();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string fileId, string ownerId, int days)
    {
        if (string.IsNullOrEmpty(fileId) || string.IsNullOrEmpty(ownerId))
        {
            TempData["Error"] = "File and User are required.";
            return RedirectToAction(nameof(Index));
        }

        await _svc.CreateShareAsync(fileId, ownerId, days);

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        await _svc.LogActionAsync(adminId, adminEmail, "ShareCreated", "Share", fileId,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = "New share link created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetFilesByUser(string userId)
    {
        // Fetch ALL files for this user (not paginated for selection)
        var files = await _svc.GetFilesAsync(1, 1000, null, userId); 
        return Json(files.Select(f => new { id = f.Id, name = f.FileName + "." + f.Extension }));
    }

    public async Task<IActionResult> Details(string id)
    {
        var share = await _svc.GetShareByIdAsync(id);
        if (share == null) return NotFound();

        var owner = await _svc.GetUserByIdAsync(share.OwnerUserId);
        var file = await _svc.GetFileByIdAsync(share.FileId);

        var model = new ShareRowViewModel
        {
            Id = share.Id,
            FileId = share.FileId,
            FileName = file?.FileName ?? "Unknown File",
            Token = share.Token,
            OwnerUserId = share.OwnerUserId,
            OwnerEmail = owner?.Email ?? "Unknown Owner",
            ExpiresAt = share.ExpiresAt,
            AllowDownload = share.AllowDownload,
            AccessCount = share.AccessCount,
            IsRevoked = share.IsRevoked,
            CreatedAt = share.CreatedAt
        };

        // Get site base URL for the link
        var request = HttpContext.Request;
        // Assuming the main app is on the same domain but maybe different port?
        // Or I can just show the relative path or use a placeholder if unsure.
        // Usually, the share link is like /s/{token}
        ViewData["ShareLink"] = $"{request.Scheme}://{request.Host}/s/{share.Token}";

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStatus(string id)
    {
        await _svc.ToggleShareStatusAsync(id);
        TempData["Success"] = "Share status updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        await _svc.DeleteShareAsync(id);
        TempData["Success"] = "Share link deleted.";
        return RedirectToAction(nameof(Index));
    }
}
