using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FileVaultAdmin.Helpers;
using FileVaultAdmin.Models.ViewModels;
using FileVaultAdmin.Services;

namespace FileVaultAdmin.Controllers;

[Authorize]
public class AnalyticsController : Controller
{
    private readonly AdminService _svc;

    public AnalyticsController(AdminService svc) => _svc = svc;

    public async Task<IActionResult> Index()
    {
        var topStorageUsers = await _svc.GetTopStorageUsersAsync(5);
        var topShares = await _svc.GetTopAccessedSharesAsync(5);
        var recentLogs = await _svc.GetAuditLogsAsync(1, 10);

        var model = new AnalyticsViewModel
        {
            // User Analytics
            TotalUsers = await _svc.CountUsersAsync(),
            ActiveUsers = await _svc.CountActiveUsersAsync(),
            DisabledUsers = await _svc.CountDisabledUsersAsync(),
            AdminUsers = await _svc.CountAdminUsersAsync(),
            UsersPerMonth = await _svc.GetUsersPerMonthAsync(),
            TopStorageUsers = topStorageUsers.Select(u => new UserStorageRow
            {
                Id = u.Id, FullName = u.FullName, Email = u.Email,
                StorageUsedBytes = u.StorageUsedBytes,
                StorageFormatted = FormatHelpers.FormatFileSize(u.StorageUsedBytes)
            }).ToList(),

            // File Analytics
            TotalFiles = await _svc.CountTotalFilesAsync(),
            ActiveFiles = await _svc.CountTotalFilesAsync() - await _svc.CountTrashedFilesAsync(),
            TrashedFiles = await _svc.CountTrashedFilesAsync(),
            TotalStorageBytes = await _svc.GetTotalStorageAsync(),
            TotalStorageFormatted = FormatHelpers.FormatFileSize(await _svc.GetTotalStorageAsync()),
            FileTypeBreakdown = await _svc.GetFileTypeBreakdownAsync(),
            FilesPerMonth = await _svc.GetFilesPerMonthAsync(),

            // Share Analytics
            TotalShares = await _svc.CountActiveSharesAsync() + await _svc.CountExpiredSharesAsync() + await _svc.CountRevokedSharesAsync(),
            ActiveShares = await _svc.CountActiveSharesAsync(),
            ExpiredShares = await _svc.CountExpiredSharesAsync(),
            RevokedShares = await _svc.CountRevokedSharesAsync(),
            TotalAccessCount = await _svc.GetTotalShareAccessCountAsync(),
            TopAccessedShares = topShares.Select(s => new ShareAccessRow
            {
                Id = s.Id, Token = s.Token, AccessCount = s.AccessCount
            }).ToList(),

            // Folder Analytics
            TotalFolders = await _svc.CountFoldersAsync(),
            ActiveFolders = await _svc.CountActiveFoldersAsync(),
            DeletedFolders = await _svc.CountDeletedFoldersAsync(),

            // Access Analytics
            ActionBreakdown = await _svc.GetActionBreakdownAsync(),
            RecentActivity = recentLogs.Select(l => new AuditLogRowViewModel
            {
                Id = l.Id, UserId = l.UserId, UserEmail = l.UserEmail,
                Action = l.Action, TargetType = l.TargetType,
                TargetId = l.TargetId, IpAddress = l.IpAddress, CreatedAt = l.CreatedAt
            }).ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> Users()
    {
        var model = new AnalyticsViewModel
        {
            TotalUsers = await _svc.CountUsersAsync(),
            ActiveUsers = await _svc.CountActiveUsersAsync(),
            DisabledUsers = await _svc.CountDisabledUsersAsync(),
            AdminUsers = await _svc.CountUsersAsync(role: "Admin"),
            UsersPerMonth = await _svc.GetUsersPerMonthAsync(),
            DisabledUsersList = await _svc.GetDisabledUsersSummaryAsync(10)
        };
        return View(model);
    }

    public async Task<IActionResult> Files()
    {
        var totalBytes = await _svc.GetTotalStorageAsync();
        var largeFiles = await _svc.GetLargeFilesAsync(10);
        var owners = (await _svc.GetAllUsersBasicAsync()).ToDictionary(u => u.Id, u => u.Email);

        var model = new AnalyticsViewModel
        {
            TotalFiles = await _svc.CountTotalFilesAsync(),
            TrashedFiles = await _svc.CountTrashedFilesAsync(),
            TotalStorageBytes = totalBytes,
            TotalStorageFormatted = FormatHelpers.FormatFileSize(totalBytes),
            FileTypeBreakdown = await _svc.GetFileTypeBreakdownAsync(),
            FilesPerMonth = await _svc.GetFilesPerMonthAsync(),
            LargeFilesList = largeFiles.Select(f => new FileRowViewModel
            {
                Id = f.Id, FileName = f.FileName, Extension = f.Extension, SizeBytes = f.SizeBytes,
                SizeFormatted = FormatHelpers.FormatFileSize(f.SizeBytes), CreatedAt = f.CreatedAt,
                OwnerEmail = owners.GetValueOrDefault(f.OwnerUserId)
            }).ToList()
        };
        return View(model);
    }

    public async Task<IActionResult> Shares()
    {
        var topShares = await _svc.GetTopAccessedSharesAsync(20);
        var expiredShares = await _svc.GetExpiredSharesSummaryAsync(10);
        var owners = (await _svc.GetAllUsersBasicAsync()).ToDictionary(u => u.Id, u => u.Email);

        var model = new AnalyticsViewModel
        {
            TotalShares = await _svc.CountActiveSharesAsync() + await _svc.CountExpiredSharesAsync() + await _svc.CountRevokedSharesAsync(),
            ActiveShares = await _svc.CountActiveSharesAsync(),
            ExpiredShares = await _svc.CountExpiredSharesAsync(),
            RevokedShares = await _svc.CountRevokedSharesAsync(),
            TotalAccessCount = await _svc.GetTotalShareAccessCountAsync(),
            TopAccessedShares = topShares.Select(s => new ShareAccessRow
            {
                Id = s.Id, Token = s.Token, AccessCount = s.AccessCount
            }).ToList(),
            ExpiredSharesList = expiredShares.Select(s => new ShareRowViewModel
            {
                Id = s.Id, Token = s.Token, ExpiresAt = s.ExpiresAt, CreatedAt = s.CreatedAt,
                OwnerEmail = owners.GetValueOrDefault(s.OwnerUserId)
            }).ToList()
        };
        return View(model);
    }

    public async Task<IActionResult> Folders()
    {
        var trashedFolders = await _svc.GetTrashedFoldersAsync();
        var owners = (await _svc.GetAllUsersBasicAsync()).ToDictionary(u => u.Id, u => u.Email);

        var model = new AnalyticsViewModel
        {
            TotalFolders = await _svc.CountFoldersAsync(),
            ActiveFolders = await _svc.CountFoldersAsync() - await _svc.CountTrashedFoldersAsync(),
            DeletedFolders = await _svc.CountTrashedFoldersAsync(),
            TrashedFoldersList = trashedFolders.Select(f => new FolderRowViewModel
            {
                Id = f.Id, Name = f.Name, CreatedAt = f.CreatedAt,
                OwnerEmail = owners.GetValueOrDefault(f.OwnerUserId)
            }).Take(10).ToList()
        };
        return View(model);
    }

    public async Task<IActionResult> Storage()
    {
        var topUsers = await _svc.GetTopStorageUsersAsync(50);
        var totalBytes = await _svc.GetTotalStorageAsync();
        var model = new AnalyticsViewModel
        {
            TotalStorageBytes = totalBytes,
            TotalStorageFormatted = FormatHelpers.FormatFileSize(totalBytes),
            TopStorageUsers = topUsers.Select(u => new UserStorageRow
            {
                Id = u.Id, FullName = u.FullName, Email = u.Email,
                StorageUsedBytes = u.StorageUsedBytes,
                StorageFormatted = FormatHelpers.FormatFileSize(u.StorageUsedBytes)
            }).ToList()
        };
        return View(model);
    }

    public async Task<IActionResult> Access(string? actionType = null)
    {
        var logs = await _svc.GetAuditLogsAsync(1, 100, actionType);
        var model = new AnalyticsViewModel
        {
            ActionBreakdown = await _svc.GetActionBreakdownAsync(),
            RecentActivity = logs.Select(l => new AuditLogRowViewModel
            {
                Id = l.Id, UserEmail = l.UserEmail, Action = l.Action,
                TargetType = l.TargetType, CreatedAt = l.CreatedAt
            }).ToList()
        };
        ViewBag.SelectedAction = actionType;
        return View(model);
    }
}
