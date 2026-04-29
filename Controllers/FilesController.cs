using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FileVaultAdmin.Helpers;
using FileVaultAdmin.Models.ViewModels;
using FileVaultAdmin.Services;

namespace FileVaultAdmin.Controllers;

[Authorize]
public class FilesController : Controller
{
    private readonly AdminService _svc;

    public FilesController(AdminService svc) => _svc = svc;

    public async Task<IActionResult> Index(int page = 1, string? search = null, string? owner = null, string? folderId = null)
    {
        var pageSize = 50;
        var files = await _svc.GetFilesAsync(page, pageSize, search, owner, folderId);
        var total = await _svc.CountFilesAsync(search, owner, folderId);

        // Resolve owner emails
        var ownerIds = files.Select(f => f.OwnerUserId).Distinct().ToList();
        var owners = new Dictionary<string, string>();
        foreach (var oid in ownerIds)
        {
            var user = await _svc.GetUserByIdAsync(oid);
            if (user != null) owners[oid] = user.Email;
        }

        var model = new FileListViewModel
        {
            Files = files.Select(f => new FileRowViewModel
            {
                Id = f.Id, FileName = f.FileName, Extension = f.Extension,
                SizeBytes = f.SizeBytes, SizeFormatted = FormatHelpers.FormatFileSize(f.SizeBytes),
                OwnerUserId = f.OwnerUserId, OwnerEmail = owners.GetValueOrDefault(f.OwnerUserId),
                IsDeleted = f.IsDeleted, CreatedAt = f.CreatedAt
            }).ToList(),
            Page = page, PageSize = pageSize, TotalFiles = total,
            SearchTerm = search, OwnerFilter = owner, FolderFilter = folderId
        };

        if (!string.IsNullOrEmpty(folderId))
        {
            var folder = await _svc.GetFolderByIdAsync(folderId);
            ViewBag.CurrentFolderName = folder?.Name;
        }

        ViewBag.AllUsers = await _svc.GetAllUsersBasicAsync();
        ViewBag.AllFolders = await _svc.GetFoldersAsync(1, 1000); // Get all for selection

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Upload(string ownerId, string? folderId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please select a file to upload.";
            return RedirectToAction(nameof(Index));
        }

        using var stream = file.OpenReadStream();
        await _svc.UploadFileAsync(ownerId, file.FileName, file.ContentType, stream, file.Length, folderId);

        TempData["Success"] = "File uploaded successfully.";
        return RedirectToAction(nameof(Index), new { folderId });
    }

    [HttpPost]
    public async Task<IActionResult> Update(string id, string fileName)
    {
        await _svc.UpdateFileAsync(id, fileName);
        TempData["Success"] = "File renamed.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DeletePermanent(string id)
    {
        await _svc.PermanentDeleteFileAsync(id);
        TempData["Success"] = "File permanently removed.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetDetails(string id)
    {
        var file = await _svc.GetFileByIdAsync(id);
        if (file == null) return NotFound();
        return Json(new { file.Id, file.FileName, file.Extension, file.SizeBytes, file.IsDeleted });
    }

    [HttpGet]
    public async Task<IActionResult> ViewFile(string id)
    {
        var file = await _svc.GetFileByIdAsync(id);
        if (file == null) return NotFound();

        var owner = await _svc.GetUserByIdAsync(file.OwnerUserId);
        
        var model = new FileRowViewModel
        {
            Id = file.Id,
            FileName = file.FileName,
            Extension = file.Extension,
            SizeBytes = file.SizeBytes,
            SizeFormatted = FormatHelpers.FormatFileSize(file.SizeBytes),
            OwnerUserId = file.OwnerUserId,
            OwnerEmail = owner?.Email,
            IsDeleted = file.IsDeleted,
            CreatedAt = file.CreatedAt,
            GridFsFileId = file.GridFsFileId, // Need this for streaming
            ContentType = file.ContentType
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Stream(string id)
    {
        var file = await _svc.GetFileByIdAsync(id);
        if (file == null) return NotFound();

        var stream = await _svc.DownloadFileAsync(file.GridFsFileId);
        if (stream == null) return NotFound();

        return File(stream, file.ContentType);
    }

    [HttpGet]
    public async Task<IActionResult> GetArchiveContents(string id)
    {
        var file = await _svc.GetFileByIdAsync(id);
        if (file == null) return NotFound();
        var structure = await _svc.GetArchiveStructureAsync(file.GridFsFileId);
        return Json(structure);
    }
}
