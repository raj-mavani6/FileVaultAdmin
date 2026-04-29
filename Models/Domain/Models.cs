using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FileVaultAdmin.Models.Domain;

[BsonIgnoreExtraElements]
public class AppUser
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("fullName")]
    public string FullName { get; set; } = null!;

    [BsonElement("email")]
    public string Email { get; set; } = null!;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = null!;

    [BsonElement("emailConfirmed")]
    public bool EmailConfirmed { get; set; }

    [BsonElement("roles")]
    public List<string> Roles { get; set; } = new() { "User" };

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("storageUsedBytes")]
    public long StorageUsedBytes { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[BsonIgnoreExtraElements]
public class FileItem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("ownerUserId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string OwnerUserId { get; set; } = null!;

    [BsonElement("folderId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? FolderId { get; set; }

    [BsonElement("fileName")]
    public string FileName { get; set; } = null!;

    [BsonElement("extension")]
    public string Extension { get; set; } = string.Empty;

    [BsonElement("contentType")]
    public string ContentType { get; set; } = "application/octet-stream";

    [BsonElement("sizeBytes")]
    public long SizeBytes { get; set; }

    [BsonElement("gridFsFileId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string GridFsFileId { get; set; } = null!;

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; set; }

    [BsonElement("deletedAt")]
    public DateTime? DeletedAt { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[BsonIgnoreExtraElements]
public class ShareLink
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("fileId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string FileId { get; set; } = null!;

    [BsonElement("ownerUserId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string OwnerUserId { get; set; } = null!;

    [BsonElement("token")]
    public string Token { get; set; } = null!;

    [BsonElement("expiresAt")]
    public DateTime? ExpiresAt { get; set; }

    [BsonElement("allowDownload")]
    public bool AllowDownload { get; set; } = true;

    [BsonElement("accessCount")]
    public int AccessCount { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isRevoked")]
    public bool IsRevoked { get; set; }
}

[BsonIgnoreExtraElements]
public class AuditLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? UserId { get; set; }

    [BsonElement("userEmail")]
    public string? UserEmail { get; set; }

    [BsonElement("action")]
    public string Action { get; set; } = null!;

    [BsonElement("targetType")]
    public string? TargetType { get; set; }

    [BsonElement("targetId")]
    public string? TargetId { get; set; }

    [BsonElement("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    [BsonElement("ipAddress")]
    public string? IpAddress { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[BsonIgnoreExtraElements]
public class Folder
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("ownerUserId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string OwnerUserId { get; set; } = null!;

    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

[BsonIgnoreExtraElements]
public class UploadSession
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!;

    [BsonElement("fileName")]
    public string FileName { get; set; } = null!;

    [BsonElement("totalSize")]
    public long TotalSize { get; set; }

    [BsonElement("status")]
    public int Status { get; set; }

    [BsonElement("startedAt")]
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
}
