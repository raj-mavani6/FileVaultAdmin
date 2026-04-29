using System.ComponentModel.DataAnnotations;
using FileVaultAdmin.Models.Domain;

namespace FileVaultAdmin.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; }
}

public class DashboardViewModel
{
    public long TotalUsers { get; set; }
    public long ActiveUsers { get; set; }
    public long DisabledUsers { get; set; }
    public long TotalFiles { get; set; }
    public long TotalFolders { get; set; }
    public long TotalStorageBytes { get; set; }
    public string TotalStorageFormatted { get; set; } = "0 B";
    public long ActiveShareLinks { get; set; }
    public long TotalAuditLogs { get; set; }
    public long TrashedFiles { get; set; }
    public long ActiveUploads { get; set; }
    public List<UserRowViewModel> RecentUsers { get; set; } = new();
    public List<AuditLogRowViewModel> RecentLogs { get; set; } = new();
    public List<FileRowViewModel> RecentFiles { get; set; } = new();
    public Dictionary<string, long> FileTypeBreakdown { get; set; } = new();
}

public class UserRowViewModel
{
    public string Id { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public long StorageUsedBytes { get; set; }
    public string StorageFormatted { get; set; } = "0 B";
    public DateTime CreatedAt { get; set; }
}

public class UserDetailsViewModel : UserRowViewModel
{
    public string PasswordHash { get; set; } = null!;
    public long FileCount { get; set; }
    public long FolderCount { get; set; }
    public List<AuditLogRowViewModel> RecentActivity { get; set; } = new();
}

public class UserListViewModel
{
    public List<UserRowViewModel> Users { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public long TotalUsers { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalUsers / PageSize);
    public string? SearchTerm { get; set; }
    public string? RoleFilter { get; set; }
    public string? StatusFilter { get; set; }
}

public class AuditLogRowViewModel
{
    public string Id { get; set; } = null!;
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Action { get; set; } = null!;
    public string? TargetType { get; set; }
    public string? TargetId { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AuditLogListViewModel
{
    public List<AuditLogRowViewModel> Logs { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public long TotalLogs { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalLogs / PageSize);
    public string? ActionFilter { get; set; }
    public string? UserFilter { get; set; }
    public List<string> AvailableActions { get; set; } = new();
}

public class FileRowViewModel
{
    public string Id { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string Extension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string GridFsFileId { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string SizeFormatted { get; set; } = "0 B";
    public string OwnerUserId { get; set; } = null!;
    public string? OwnerEmail { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class FileListViewModel
{
    public List<FileRowViewModel> Files { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public long TotalFiles { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalFiles / PageSize);
    public string? SearchTerm { get; set; }
    public string? OwnerFilter { get; set; }
    public string? FolderFilter { get; set; }
}

public class ShareListViewModel
{
    public List<ShareRowViewModel> Shares { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public long TotalShares { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalShares / PageSize);
    public string? SearchTerm { get; set; }
}

public class ShareRowViewModel
{
    public string Id { get; set; } = null!;
    public string FileId { get; set; } = null!;
    public string? FileName { get; set; }
    public string Token { get; set; } = null!;
    public string OwnerUserId { get; set; } = null!;
    public string? OwnerEmail { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool AllowDownload { get; set; }
    public int AccessCount { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SystemSettingsViewModel
{
    public string DatabaseName { get; set; } = null!;
    public string ConnectionStatus { get; set; } = "Unknown";
    public long DatabaseSizeBytes { get; set; }
    public string DatabaseSizeFormatted { get; set; } = "0 B";
    public int CollectionCount { get; set; }
}

// ===== Analytics =====
public class AnalyticsViewModel
{
    // User Analytics
    public long TotalUsers { get; set; }
    public long ActiveUsers { get; set; }
    public long DisabledUsers { get; set; }
    public long AdminUsers { get; set; }
    public Dictionary<string, long> UsersPerMonth { get; set; } = new();
    public List<UserStorageRow> TopStorageUsers { get; set; } = new();

    // File Analytics
    public long TotalFiles { get; set; }
    public long ActiveFiles { get; set; }
    public long TrashedFiles { get; set; }
    public long TotalStorageBytes { get; set; }
    public string TotalStorageFormatted { get; set; } = "0 B";
    public Dictionary<string, long> FileTypeBreakdown { get; set; } = new();
    public Dictionary<string, long> FilesPerMonth { get; set; } = new();

    // Share Analytics
    public long TotalShares { get; set; }
    public long ActiveShares { get; set; }
    public long ExpiredShares { get; set; }
    public long RevokedShares { get; set; }
    public long TotalAccessCount { get; set; }
    public List<ShareAccessRow> TopAccessedShares { get; set; } = new();

    // Folder Analytics
    public long TotalFolders { get; set; }
    public long ActiveFolders { get; set; }
    public long DeletedFolders { get; set; }

    // Access Analytics
    public Dictionary<string, long> ActionBreakdown { get; set; } = new();
    public List<AuditLogRowViewModel> RecentActivity { get; set; } = new();

    // Bottom Insight Lists
    public List<AppUser> DisabledUsersList { get; set; } = new();
    public List<FileRowViewModel> LargeFilesList { get; set; } = new();
    public List<ShareRowViewModel> ExpiredSharesList { get; set; } = new();
    public List<FolderRowViewModel> TrashedFoldersList { get; set; } = new();
}

public class UserStorageRow
{
    public string Id { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public long StorageUsedBytes { get; set; }
    public string StorageFormatted { get; set; } = "0 B";
}

public class ShareAccessRow
{
    public string Id { get; set; } = null!;
    public string? FileName { get; set; }
    public string Token { get; set; } = null!;
    public int AccessCount { get; set; }
}

// ===== Folders =====
public class FolderRowViewModel
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string OwnerUserId { get; set; } = null!;
    public string? OwnerEmail { get; set; }
    public bool IsDeleted { get; set; }
    public long FileCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class FolderListViewModel
{
    public List<FolderRowViewModel> Folders { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public long TotalFolders { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalFolders / PageSize);
    public string? SearchTerm { get; set; }
    public string? OwnerFilter { get; set; }
}

public class TrashViewModel
{
    public List<FileRowViewModel> Files { get; set; } = new();
    public List<FolderRowViewModel> Folders { get; set; } = new();
}

public class ExtensionStat
{
    public string Extension { get; set; } = string.Empty;
    public long Count { get; set; }
}

