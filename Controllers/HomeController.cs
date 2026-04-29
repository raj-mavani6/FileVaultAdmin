using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FileVaultAdmin.Helpers;
using FileVaultAdmin.Models.ViewModels;
using FileVaultAdmin.Services;

namespace FileVaultAdmin.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly AdminService _svc;

    public HomeController(AdminService svc) => _svc = svc;

    public async Task<IActionResult> Index()
    {
        var totalUsers = await _svc.CountUsersAsync();
        var activeUsers = await _svc.CountActiveUsersAsync();
        var disabledUsers = await _svc.CountDisabledUsersAsync();
        var totalFiles = await _svc.CountTotalFilesAsync();
        var totalFolders = await _svc.CountFoldersAsync();
        var totalStorage = await _svc.GetTotalStorageAsync();
        var activeShares = await _svc.CountActiveSharesAsync();
        var totalLogs = await _svc.CountAuditLogsAllAsync();
        var trashedFiles = await _svc.CountTrashedFilesAsync();
        var activeUploads = await _svc.CountActiveUploadsAsync();
        var recentUsers = await _svc.GetUsersAsync(1, 20);
        var recentLogs = await _svc.GetAuditLogsAsync(1, 20);
        var recentFiles = await _svc.GetFilesAsync(1, 20);
        var typeBreakdown = await _svc.GetFileTypeBreakdownAsync();

        var model = new DashboardViewModel
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            DisabledUsers = disabledUsers,
            TotalFiles = totalFiles,
            TotalFolders = totalFolders,
            TotalStorageBytes = totalStorage,
            TotalStorageFormatted = FormatHelpers.FormatFileSize(totalStorage),
            ActiveShareLinks = activeShares,
            TotalAuditLogs = totalLogs,
            TrashedFiles = trashedFiles,
            ActiveUploads = activeUploads,
            FileTypeBreakdown = typeBreakdown,
            RecentUsers = recentUsers.Select(u => new UserRowViewModel
            {
                Id = u.Id, FullName = u.FullName, Email = u.Email,
                Roles = u.Roles, IsActive = u.IsActive, EmailConfirmed = u.EmailConfirmed,
                StorageUsedBytes = u.StorageUsedBytes,
                StorageFormatted = FormatHelpers.FormatFileSize(u.StorageUsedBytes),
                CreatedAt = u.CreatedAt
            }).ToList(),
            RecentLogs = recentLogs.Select(l => new AuditLogRowViewModel
            {
                Id = l.Id, UserId = l.UserId, UserEmail = l.UserEmail,
                Action = l.Action, TargetType = l.TargetType,
                TargetId = l.TargetId, IpAddress = l.IpAddress, CreatedAt = l.CreatedAt
            }).ToList(),
            RecentFiles = recentFiles.Select(f => new FileRowViewModel
            {
                Id = f.Id, FileName = f.FileName, Extension = f.Extension,
                SizeBytes = f.SizeBytes, SizeFormatted = FormatHelpers.FormatFileSize(f.SizeBytes),
                OwnerUserId = f.OwnerUserId, IsDeleted = f.IsDeleted, CreatedAt = f.CreatedAt
            }).ToList()
        };

        return View(model);
    }
}
