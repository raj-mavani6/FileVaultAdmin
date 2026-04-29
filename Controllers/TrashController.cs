using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FileVaultAdmin.Models.ViewModels;
using FileVaultAdmin.Services;
using FileVaultAdmin.Helpers;

namespace FileVaultAdmin.Controllers;

[Authorize]
public class TrashController : Controller
{
    private readonly AdminService _svc;

    public TrashController(AdminService svc) => _svc = svc;

    public async Task<IActionResult> Index()
    {
        var files = await _svc.GetTrashedFilesAsync();
        var folders = await _svc.GetTrashedFoldersAsync();

        // Map to rows
        var fileRows = new List<FileRowViewModel>();
        foreach (var f in files)
        {
            var owner = await _svc.GetUserByIdAsync(f.OwnerUserId);
            fileRows.Add(new FileRowViewModel
            {
                Id = f.Id,
                FileName = f.FileName,
                Extension = f.Extension,
                SizeBytes = f.SizeBytes,
                OwnerUserId = f.OwnerUserId,
                OwnerEmail = owner?.Email,
                CreatedAt = f.CreatedAt,
                DeletedAt = f.DeletedAt,
                IsDeleted = true
            });
        }

        var folderRows = new List<FolderRowViewModel>();
        foreach (var f in folders)
        {
            var owner = await _svc.GetUserByIdAsync(f.OwnerUserId);
            folderRows.Add(new FolderRowViewModel
            {
                Id = f.Id,
                Name = f.Name,
                OwnerUserId = f.OwnerUserId,
                OwnerEmail = owner?.Email,
                CreatedAt = f.CreatedAt,
                IsDeleted = true
            });
        }

        var model = new TrashViewModel
        {
            Files = fileRows,
            Folders = folderRows
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> RestoreFile(string id)
    {
        await _svc.RestoreFileAsync(id);
        
        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        await _svc.LogActionAsync(adminId, adminEmail, "FileRestored", "File", id,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = "File restored successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> PermanentDeleteFile(string id)
    {
        await _svc.PermanentDeleteFileAsync(id);

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        await _svc.LogActionAsync(adminId, adminEmail, "FilePurged", "File", id,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = "File permanently purged.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> RestoreFolder(string id)
    {
        await _svc.RestoreFolderAsync(id);

        var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var adminEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        await _svc.LogActionAsync(adminId, adminEmail, "FolderRestored", "Folder", id,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        TempData["Success"] = "Folder restored successfully.";
        return RedirectToAction(nameof(Index));
    }
}
