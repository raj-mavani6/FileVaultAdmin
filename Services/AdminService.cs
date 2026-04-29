using MongoDB.Driver;
using MongoDB.Bson;
using FileVaultAdmin.Data;
using FileVaultAdmin.Models.Domain;

namespace FileVaultAdmin.Services;

public class AdminService
{
    private readonly MongoDbContext _db;

    public AdminService(MongoDbContext db) => _db = db;

    // ===== Users =====
    public async Task<List<AppUser>> GetUsersAsync(int page, int pageSize,
        string? search = null, string? role = null, string? status = null)
    {
        var builder = Builders<AppUser>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var regex = new BsonRegularExpression(search, "i");
            var searchFilter = builder.Or(
                builder.Regex(u => u.FullName, regex),
                builder.Regex(u => u.Email, regex));
                
            if (ObjectId.TryParse(search, out _))
            {
                searchFilter = builder.Or(searchFilter, builder.Eq(u => u.Id, search));
            }
            filter &= searchFilter;
        }
        if (!string.IsNullOrWhiteSpace(role))
            filter &= builder.AnyEq(u => u.Roles, role);
        if (status == "active")
            filter &= builder.Eq(u => u.IsActive, true);
        else if (status == "disabled")
            filter &= builder.Eq(u => u.IsActive, false);

        return await _db.Users.Find(filter)
            .SortByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<long> CountUsersAsync(string? search = null, string? role = null, string? status = null)
    {
        var builder = Builders<AppUser>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var regex = new BsonRegularExpression(search, "i");
            var searchFilter = builder.Or(
                builder.Regex(u => u.FullName, regex),
                builder.Regex(u => u.Email, regex));
                
            if (ObjectId.TryParse(search, out _))
            {
                searchFilter = builder.Or(searchFilter, builder.Eq(u => u.Id, search));
            }
            filter &= searchFilter;
        }
        if (!string.IsNullOrWhiteSpace(role))
            filter &= builder.AnyEq(u => u.Roles, role);
        if (status == "active")
            filter &= builder.Eq(u => u.IsActive, true);
        else if (status == "disabled")
            filter &= builder.Eq(u => u.IsActive, false);

        return await _db.Users.CountDocumentsAsync(filter);
    }

    public async Task<AppUser?> GetUserByIdAsync(string id)
        => await _db.Users.Find(u => u.Id == id).FirstOrDefaultAsync();

    public async Task<AppUser?> GetUserByEmailAsync(string email)
        => await _db.Users.Find(u => u.Email == email.ToLowerInvariant()).FirstOrDefaultAsync();

    public async Task<bool> DisableUserAsync(string id)
    {
        var update = Builders<AppUser>.Update
            .Set(u => u.IsActive, false)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);
        var result = await _db.Users.UpdateOneAsync(u => u.Id == id, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> EnableUserAsync(string id)
    {
        var update = Builders<AppUser>.Update
            .Set(u => u.IsActive, true)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);
        var result = await _db.Users.UpdateOneAsync(u => u.Id == id, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> ToggleRoleAsync(string userId, string role)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null) return false;

        if (user.Roles.Contains(role))
        {
            var update = Builders<AppUser>.Update.Pull(u => u.Roles, role);
            await _db.Users.UpdateOneAsync(u => u.Id == userId, update);
        }
        else
        {
            var update = Builders<AppUser>.Update.AddToSet(u => u.Roles, role);
            await _db.Users.UpdateOneAsync(u => u.Id == userId, update);
        }
        return true;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var result = await _db.Users.DeleteOneAsync(u => u.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<bool> CreateUserAsync(AppUser user)
    {
        user.Email = user.Email.ToLowerInvariant();
        user.CreatedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.Users.InsertOneAsync(user);
        return true;
    }

    public async Task<bool> UpdateUserAsync(string id, string fullName, string email, string? password, bool isAdmin)
    {
        var update = Builders<AppUser>.Update
            .Set(u => u.FullName, fullName)
            .Set(u => u.Email, email.ToLowerInvariant())
            .Set(u => u.UpdatedAt, DateTime.UtcNow);

        if (!string.IsNullOrWhiteSpace(password))
        {
            update = update.Set(u => u.PasswordHash, BCrypt.Net.BCrypt.HashPassword(password));
        }

        var roles = new List<string> { "User" };
        if (isAdmin) roles.Add("Admin");
        
        update = update.Set(u => u.Roles, roles);

        var result = await _db.Users.UpdateOneAsync(u => u.Id == id, update);
        return result.ModifiedCount > 0;
    }

    public async Task<long> GetUserFilesCountAsync(string userId)
        => await _db.Files.CountDocumentsAsync(f => f.OwnerUserId == userId && !f.IsDeleted);

    public async Task<long> GetUserFoldersCountAsync(string userId)
        => await _db.Folders.CountDocumentsAsync(f => f.OwnerUserId == userId);

    public async Task<List<AuditLog>> GetUserRecentActivityAsync(string userId, int limit = 10)
        => await _db.AuditLogs.Find(l => l.UserId == userId)
            .SortByDescending(l => l.CreatedAt)
            .Limit(limit)
            .ToListAsync();

    // ===== Files =====
    public async Task<List<FileItem>> GetFilesAsync(int page, int pageSize,
        string? search = null, string? owner = null, string? folderId = null)
    {
        var builder = Builders<FileItem>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(search))
            filter &= builder.Regex(f => f.FileName, new BsonRegularExpression(search, "i"));
        if (!string.IsNullOrWhiteSpace(owner))
        {
            if (ObjectId.TryParse(owner, out _))
                filter &= builder.Eq(f => f.OwnerUserId, owner);
            else
                filter &= builder.Regex(f => f.OwnerUserId, new BsonRegularExpression(owner, "i"));
        }
        if (!string.IsNullOrWhiteSpace(folderId) && ObjectId.TryParse(folderId, out _))
        {
            filter &= builder.Eq(f => f.FolderId, folderId);
        }

        return await _db.Files.Find(filter)
            .SortByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<long> CountFilesAsync(string? search = null, string? owner = null, string? folderId = null)
    {
        var builder = Builders<FileItem>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(search))
            filter &= builder.Regex(f => f.FileName, new BsonRegularExpression(search, "i"));
        if (!string.IsNullOrWhiteSpace(owner) && ObjectId.TryParse(owner, out _))
            filter &= builder.Eq(f => f.OwnerUserId, owner);
        if (!string.IsNullOrWhiteSpace(folderId) && ObjectId.TryParse(folderId, out _))
            filter &= builder.Eq(f => f.FolderId, folderId);

        return await _db.Files.CountDocumentsAsync(filter);
    }

    public async Task<FileItem?> GetFileByIdAsync(string id)
        => await _db.Files.Find(f => f.Id == id).FirstOrDefaultAsync();

    public async Task<bool> UpdateFileAsync(string id, string fileName)
    {
        var update = Builders<FileItem>.Update
            .Set(f => f.FileName, fileName)
            .Set(f => f.UpdatedAt, DateTime.UtcNow);
        var result = await _db.Files.UpdateOneAsync(f => f.Id == id, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> PermanentDeleteFileAsync(string id)
    {
        var file = await _db.Files.Find(f => f.Id == id).FirstOrDefaultAsync();
        if (file == null) return false;

        // 1. Delete GridFS binary
        if (!string.IsNullOrEmpty(file.GridFsFileId))
        {
            await _db.FilesBucket.DeleteAsync(new ObjectId(file.GridFsFileId));
        }

        // 2. Decrement user storage
        await _db.Users.UpdateOneAsync(
            u => u.Id == file.OwnerUserId,
            Builders<AppUser>.Update.Inc(u => u.StorageUsedBytes, -file.SizeBytes)
        );

        // 3. Delete DB record
        var result = await _db.Files.DeleteOneAsync(f => f.Id == id);
        return result.DeletedCount > 0;
    }

    // ===== Shares =====
    public async Task<ShareLink?> GetShareByIdAsync(string id)
        => await _db.ShareLinks.Find(s => s.Id == id).FirstOrDefaultAsync();

    public async Task<List<ShareLink>> GetSharesAsync(int page, int pageSize, string? search = null)
    {
        var builder = Builders<ShareLink>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var regex = new BsonRegularExpression(search, "i");
            filter &= builder.Or(
                builder.Regex(s => s.Token, regex),
                builder.Regex(s => s.OwnerUserId, regex));
        }

        return await _db.ShareLinks.Find(filter)
            .SortByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<long> CountSharesAsync(string? search = null)
    {
        var builder = Builders<ShareLink>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(search))
        {
            var regex = new BsonRegularExpression(search, "i");
            filter &= builder.Or(
                builder.Regex(s => s.Token, regex),
                builder.Regex(s => s.OwnerUserId, regex));
        }

        return await _db.ShareLinks.CountDocumentsAsync(filter);
    }

    public async Task CreateShareAsync(string fileId, string ownerId, int days)
    {
        var share = new ShareLink
        {
            FileId = fileId,
            OwnerUserId = ownerId,
            Token = Guid.NewGuid().ToString("N").Substring(0, 16),
            AllowDownload = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = days > 0 ? DateTime.UtcNow.AddDays(days) : null,
            IsRevoked = false,
            AccessCount = 0
        };
        await _db.ShareLinks.InsertOneAsync(share);
    }

    public async Task<bool> RevokeShareAsync(string id)
    {
        var update = Builders<ShareLink>.Update.Set(s => s.IsRevoked, true);
        var result = await _db.ShareLinks.UpdateOneAsync(s => s.Id == id, update);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> ToggleShareStatusAsync(string id)
    {
        var share = await _db.ShareLinks.Find(s => s.Id == id).FirstOrDefaultAsync();
        if (share == null) return false;

        var update = Builders<ShareLink>.Update.Set(s => s.IsRevoked, !share.IsRevoked);
        await _db.ShareLinks.UpdateOneAsync(s => s.Id == id, update);
        return true;
    }

    public async Task<bool> DeleteShareAsync(string id)
    {
        var result = await _db.ShareLinks.DeleteOneAsync(s => s.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<System.IO.Stream?> DownloadFileAsync(string gridFsId)
    {
        try
        {
            return await _db.FilesBucket.OpenDownloadStreamAsync(new ObjectId(gridFsId));
        }
        catch { return null; }
    }

    public async Task<List<string>> GetArchiveStructureAsync(string gridFsId)
    {
        var files = new List<string>();
        try
        {
            using var stream = await DownloadFileAsync(gridFsId);
            if (stream == null) return files;

            using var reader = SharpCompress.Readers.ReaderFactory.Open(stream);
            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory)
                {
                    files.Add($"{reader.Entry.Key} ({(reader.Entry.Size / 1024.0):0.##} KB)");
                }
                if (files.Count >= 50) break;
            }
        }
        catch { /* Error reading or unsupported format */ }
        return files;
    }

    // ===== Audit Logs =====
    public async Task<List<AuditLog>> GetAuditLogsAsync(int page, int pageSize,
        string? action = null, string? userFilter = null)
    {
        var builder = Builders<AuditLog>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(action))
            filter &= builder.Regex(l => l.Action, new BsonRegularExpression(action, "i"));
            
        if (!string.IsNullOrWhiteSpace(userFilter))
        {
            var regex = new BsonRegularExpression(userFilter, "i");
            var userSearch = builder.Or(
                builder.Regex(l => l.UserEmail, regex),
                builder.Regex(l => l.UserId, regex),
                builder.Regex(l => l.TargetId, regex)
            );
            filter &= userSearch;
        }

        return await _db.AuditLogs.Find(filter)
            .SortByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<long> CountAuditLogsAsync(string? action = null, string? userFilter = null)
    {
        var builder = Builders<AuditLog>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(action))
            filter &= builder.Regex(l => l.Action, new BsonRegularExpression(action, "i"));
            
        if (!string.IsNullOrWhiteSpace(userFilter))
        {
            var regex = new BsonRegularExpression(userFilter, "i");
            var userSearch = builder.Or(
                builder.Regex(l => l.UserEmail, regex),
                builder.Regex(l => l.UserId, regex),
                builder.Regex(l => l.TargetId, regex)
            );
            filter &= userSearch;
        }

        return await _db.AuditLogs.CountDocumentsAsync(filter);
    }

    public async Task<List<string>> GetDistinctActionsAsync()
    {
        try {
            var actions = await _db.AuditLogs.Distinct(l => l.Action, Builders<AuditLog>.Filter.Empty).ToListAsync();
            return actions ?? new List<string>();
        } catch { return new List<string>(); }
    }

    public async Task LogActionAsync(string? userId, string? email, string action,
        string? targetType = null, string? targetId = null, string? ip = null)
    {
        await _db.AuditLogs.InsertOneAsync(new AuditLog
        {
            UserId = ObjectId.TryParse(userId, out _) ? userId : null,
            UserEmail = email,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            IpAddress = ip
        });
    }

    // ===== Dashboard Stats =====
    public async Task<long> CountActiveUsersAsync()
        => await _db.Users.CountDocumentsAsync(u => u.IsActive);

    public async Task<long> CountDisabledUsersAsync()
        => await _db.Users.CountDocumentsAsync(u => !u.IsActive);

    public async Task<long> CountTrashedFoldersAsync()
        => await _db.Folders.CountDocumentsAsync(f => f.IsDeleted);

    public async Task<long> CountTotalFilesAsync()
        => await _db.Files.CountDocumentsAsync(_ => true);

    public async Task<long> CountTrashedFilesAsync()
        => await _db.Files.CountDocumentsAsync(f => f.IsDeleted);

    public async Task<long> CountFoldersAsync()
        => await _db.Folders.CountDocumentsAsync(_ => true);

    public async Task<long> CountActiveSharesAsync()
        => await _db.ShareLinks.CountDocumentsAsync(s => !s.IsRevoked);

    public async Task<long> CountAuditLogsAllAsync()
        => await _db.AuditLogs.CountDocumentsAsync(_ => true);

    public async Task<long> CountActiveUploadsAsync()
        => await _db.UploadSessions.CountDocumentsAsync(s => s.Status == 0);

    public async Task<long> GetTotalStorageAsync()
    {
        try
        {
            var pipeline = new BsonDocument[]
            {
                new("$match", new BsonDocument("isDeleted", new BsonDocument("$ne", true))),
                new("$group", new BsonDocument
                {
                    { "_id", BsonNull.Value },
                    { "total", new BsonDocument("$sum", "$sizeBytes") }
                })
            };
            var cursor = await _db.Files.AggregateAsync<BsonDocument>(pipeline);
            var result = await cursor.FirstOrDefaultAsync();
            if (result == null || !result.Contains("total")) return 0;
            
            var totalValue = result["total"];
            if (totalValue.IsBsonNull) return 0;
            if (totalValue.IsDouble) return (long)totalValue.AsDouble;
            if (totalValue.IsInt32) return (long)totalValue.AsInt32;
            if (totalValue.IsInt64) return totalValue.AsInt64;
            return 0;
        }
        catch { return 0; }
    }

    public async Task<Dictionary<string, long>> GetFileTypeBreakdownAsync()
    {
        var pipeline = new BsonDocument[]
        {
            new("$match", new BsonDocument("isDeleted", false)),
            new("$group", new BsonDocument
            {
                { "_id", "$extension" },
                { "count", new BsonDocument("$sum", 1) }
            }),
            new("$sort", new BsonDocument("count", -1)),
            new("$limit", 20)
        };
        var cursor = await _db.Files.AggregateAsync<BsonDocument>(pipeline);
        var results = await cursor.ToListAsync();
        return results.ToDictionary(
            r => r["_id"].IsBsonNull ? "unknown" : r["_id"].AsString,
            r => r["count"].ToInt64());
    }

    public async Task<string> UploadFileAsync(string userId, string fileName, string contentType, System.IO.Stream stream, long sizeBytes, string? folderId = null)
    {
        var gridFsId = await _db.FilesBucket.UploadFromStreamAsync(fileName, stream);
        var fileItem = new FileItem
        {
            OwnerUserId = userId,
            FolderId = folderId,
            FileName = fileName,
            Extension = System.IO.Path.GetExtension(fileName),
            ContentType = contentType,
            SizeBytes = sizeBytes,
            GridFsFileId = gridFsId.ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        await _db.Files.InsertOneAsync(fileItem);

        // Increment user storage
        await _db.Users.UpdateOneAsync(
            u => u.Id == userId,
            Builders<AppUser>.Update.Inc(u => u.StorageUsedBytes, sizeBytes)
        );

        return fileItem.Id;
    }

    public async Task<List<AppUser>> GetAllUsersBasicAsync()
    {
        return await _db.Users.Find(_ => true)
            .Project<AppUser>(Builders<AppUser>.Projection.Include("fullName").Include("email"))
            .ToListAsync();
    }

    // ===== Analytics Methods =====
    public async Task<long> CountAdminUsersAsync()
    {
        return await _db.Users.CountDocumentsAsync(Builders<AppUser>.Filter.AnyEq(u => u.Roles, "Admin"));
    }

    public async Task<Dictionary<string, long>> GetUsersPerMonthAsync()
    {
        var pipeline = new BsonDocument[]
        {
            new("$group", new BsonDocument {
                { "_id", new BsonDocument { { "y", new BsonDocument("$year", "$createdAt") }, { "m", new BsonDocument("$month", "$createdAt") } } },
                { "count", new BsonDocument("$sum", 1) }
            }),
            new("$sort", new BsonDocument("_id", 1)),
            new("$limit", 12)
        };
        var cursor = await _db.Users.AggregateAsync<BsonDocument>(pipeline);
        var results = await cursor.ToListAsync();
        var dict = new Dictionary<string, long>();
        foreach (var r in results)
        {
            var id = r["_id"].AsBsonDocument;
            var key = $"{id["y"].AsInt32}-{id["m"].AsInt32:D2}";
            dict[key] = r["count"].ToInt64();
        }
        return dict;
    }

    public async Task<List<AppUser>> GetTopStorageUsersAsync(int limit = 5)
    {
        return await _db.Users.Find(_ => true)
            .SortByDescending(u => u.StorageUsedBytes)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<Dictionary<string, long>> GetFilesPerMonthAsync()
    {
        var pipeline = new BsonDocument[]
        {
            new("$group", new BsonDocument {
                { "_id", new BsonDocument { { "y", new BsonDocument("$year", "$createdAt") }, { "m", new BsonDocument("$month", "$createdAt") } } },
                { "count", new BsonDocument("$sum", 1) }
            }),
            new("$sort", new BsonDocument("_id", 1)),
            new("$limit", 12)
        };
        var cursor = await _db.Files.AggregateAsync<BsonDocument>(pipeline);
        var results = await cursor.ToListAsync();
        var dict = new Dictionary<string, long>();
        foreach (var r in results)
        {
            var id = r["_id"].AsBsonDocument;
            var key = $"{id["y"].AsInt32}-{id["m"].AsInt32:D2}";
            dict[key] = r["count"].ToInt64();
        }
        return dict;
    }

    public async Task<long> CountExpiredSharesAsync()
    {
        return await _db.ShareLinks.CountDocumentsAsync(
            Builders<ShareLink>.Filter.Lt(s => s.ExpiresAt, DateTime.UtcNow) &
            Builders<ShareLink>.Filter.Eq(s => s.IsRevoked, false));
    }

    public async Task<long> CountRevokedSharesAsync()
    {
        return await _db.ShareLinks.CountDocumentsAsync(Builders<ShareLink>.Filter.Eq(s => s.IsRevoked, true));
    }

    public async Task<long> GetTotalShareAccessCountAsync()
    {
        var pipeline = new BsonDocument[]
        {
            new("$group", new BsonDocument { { "_id", BsonNull.Value }, { "total", new BsonDocument("$sum", "$accessCount") } })
        };
        var cursor = await _db.ShareLinks.AggregateAsync<BsonDocument>(pipeline);
        var result = await cursor.FirstOrDefaultAsync();
        return result?["total"].ToInt64() ?? 0;
    }

    public async Task<List<ShareLink>> GetTopAccessedSharesAsync(int limit = 5)
    {
        return await _db.ShareLinks.Find(_ => true)
            .SortByDescending(s => s.AccessCount)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<long> CountDeletedFoldersAsync()
    {
        return await _db.Folders.CountDocumentsAsync(Builders<Folder>.Filter.Eq(f => f.IsDeleted, true));
    }

    public async Task<long> CountActiveFoldersAsync()
    {
        return await _db.Folders.CountDocumentsAsync(Builders<Folder>.Filter.Eq(f => f.IsDeleted, false));
    }

    public async Task<Dictionary<string, long>> GetActionBreakdownAsync()
    {
        try
        {
            var pipeline = new BsonDocument[]
            {
                new("$group", new BsonDocument { { "_id", "$action" }, { "count", new BsonDocument("$sum", 1) } }),
                new("$sort", new BsonDocument("count", -1)),
                new("$limit", 15)
            };
            var cursor = await _db.AuditLogs.AggregateAsync<BsonDocument>(pipeline);
            var results = await cursor.ToListAsync();
            return results.ToDictionary(
                r => r["_id"].IsBsonNull ? "Unknown" : r["_id"].AsString,
                r => r["count"].ToInt64());
        }
        catch { return new Dictionary<string, long>(); }
    }

    // ===== Folder Management =====
    public async Task<List<Folder>> GetFoldersAsync(int page, int pageSize,
        string? search = null, string? owner = null)
    {
        var builder = Builders<Folder>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(search))
            filter &= builder.Regex(f => f.Name, new BsonRegularExpression(search, "i"));
        if (!string.IsNullOrWhiteSpace(owner))
        {
            if (ObjectId.TryParse(owner, out _))
                filter &= builder.Eq(f => f.OwnerUserId, owner);
            else
                filter &= builder.Regex(f => f.OwnerUserId, new BsonRegularExpression(owner, "i"));
        }

        return await _db.Folders.Find(filter)
            .SortByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<long> CountFoldersFilteredAsync(string? search = null, string? owner = null)
    {
        var builder = Builders<Folder>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(search))
            filter &= builder.Regex(f => f.Name, new BsonRegularExpression(search, "i"));
        if (!string.IsNullOrWhiteSpace(owner))
        {
            if (ObjectId.TryParse(owner, out _))
                filter &= builder.Eq(f => f.OwnerUserId, owner);
        }

        return await _db.Folders.CountDocumentsAsync(filter);
    }

    public async Task<long> GetFilesInFolderCountAsync(string folderId)
    {
        return await _db.Files.CountDocumentsAsync(Builders<FileItem>.Filter.Eq(f => f.FolderId, folderId));
    }

    public async Task<Folder?> GetFolderByIdAsync(string id)
    {
        return await _db.Folders.Find(f => f.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateFolderAsync(string ownerId, string name)
    {
        var folder = new Folder
        {
            OwnerUserId = ownerId,
            Name = name,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
        await _db.Folders.InsertOneAsync(folder);
    }

    public async Task DeleteFolderAsync(string id)
    {
        // 1. Find all files in this folder
        var filesInFolder = await _db.Files.Find(f => f.FolderId == id).ToListAsync();
        
        // 2. Permanently delete each file (cleans up GridFS as well)
        foreach (var file in filesInFolder)
        {
            await PermanentDeleteFileAsync(file.Id);
        }

        // 3. Delete the folder itself
        await _db.Folders.DeleteOneAsync(Builders<Folder>.Filter.Eq(f => f.Id, id));
    }

    public async Task<List<FileItem>> GetTrashedFilesAsync()
    {
        return await _db.Files.Find(f => f.IsDeleted).SortByDescending(f => f.DeletedAt).ToListAsync();
    }

    public async Task<List<Folder>> GetTrashedFoldersAsync()
    {
        return await _db.Folders.Find(f => f.IsDeleted).SortByDescending(f => f.CreatedAt).ToListAsync();
    }

    public async Task<List<AppUser>> GetDisabledUsersSummaryAsync(int limit)
    {
        return await _db.Users.Find(u => !u.IsActive)
            .SortByDescending(u => u.CreatedAt)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<List<FileItem>> GetLargeFilesAsync(int limit)
    {
        return await _db.Files.Find(f => !f.IsDeleted)
            .SortByDescending(f => f.SizeBytes)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task<List<ShareLink>> GetExpiredSharesSummaryAsync(int limit)
    {
        var now = DateTime.UtcNow;
        return await _db.ShareLinks.Find(s => s.ExpiresAt < now && !s.IsRevoked)
            .SortByDescending(s => s.ExpiresAt)
            .Limit(limit)
            .ToListAsync();
    }

    public async Task RestoreFileAsync(string id)
    {
        var update = Builders<FileItem>.Update.Set(f => f.IsDeleted, false).Set(f => f.DeletedAt, null);
        await _db.Files.UpdateOneAsync(f => f.Id == id, update);
    }

    public async Task RestoreFolderAsync(string id)
    {
        var update = Builders<Folder>.Update.Set(f => f.IsDeleted, false);
        await _db.Folders.UpdateOneAsync(f => f.Id == id, update);
    }
}
