using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FileVaultAdmin.Models.ViewModels;
using FileVaultAdmin.Services;

namespace FileVaultAdmin.Controllers;

[Authorize]
public class FoldersController : Controller
{
    private readonly AdminService _svc;

    public FoldersController(AdminService svc) => _svc = svc;

    public async Task<IActionResult> Index(int page = 1, string? search = null, string? owner = null)
    {
        var pageSize = 50;
        var folders = await _svc.GetFoldersAsync(page, pageSize, search, owner);
        var total = await _svc.CountFoldersFilteredAsync(search, owner);

        var ownerIds = folders.Select(f => f.OwnerUserId).Distinct().ToList();
        var owners = new Dictionary<string, string>();
        foreach (var oid in ownerIds)
        {
            var user = await _svc.GetUserByIdAsync(oid);
            if (user != null) owners[oid] = user.Email;
        }

        var folderRows = new List<FolderRowViewModel>();
        foreach (var f in folders)
        {
            var fileCount = await _svc.GetFilesInFolderCountAsync(f.Id);
            folderRows.Add(new FolderRowViewModel
            {
                Id = f.Id,
                Name = f.Name,
                OwnerUserId = f.OwnerUserId,
                OwnerEmail = owners.GetValueOrDefault(f.OwnerUserId),
                IsDeleted = f.IsDeleted,
                FileCount = fileCount,
                CreatedAt = f.CreatedAt
            });
        }

        var model = new FolderListViewModel
        {
            Folders = folderRows,
            Page = page,
            PageSize = pageSize,
            TotalFolders = total,
            SearchTerm = search,
            OwnerFilter = owner
        };

        ViewBag.AllUsers = await _svc.GetAllUsersBasicAsync();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string ownerId, string name)
    {
        if (string.IsNullOrWhiteSpace(ownerId) || string.IsNullOrWhiteSpace(name))
        {
            TempData["Error"] = "Owner ID and Name are required.";
            return RedirectToAction(nameof(Index));
        }

        await _svc.CreateFolderAsync(ownerId, name);

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        await _svc.LogActionAsync(adminId, adminEmail, "FolderCreated", "Folder", name,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = "Folder created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        await _svc.DeleteFolderAsync(id);

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        await _svc.LogActionAsync(adminId, adminEmail, "FolderDeleted", "Folder", id,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = "Folder deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
