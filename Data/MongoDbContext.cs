using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Microsoft.Extensions.Options;
using FileVaultAdmin.Models.Domain;
using FileVaultAdmin.Models.Settings;

namespace FileVaultAdmin.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly IGridFSBucket _bucket;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
        _bucket = new GridFSBucket(_database, new GridFSBucketOptions
        {
            BucketName = "fileVaultFiles"
        });
    }

    public IMongoDatabase Database => _database;
    public IGridFSBucket FilesBucket => _bucket;
    public IMongoCollection<AppUser> Users => _database.GetCollection<AppUser>("users");
    public IMongoCollection<FileItem> Files => _database.GetCollection<FileItem>("files");
    public IMongoCollection<Folder> Folders => _database.GetCollection<Folder>("folders");
    public IMongoCollection<ShareLink> ShareLinks => _database.GetCollection<ShareLink>("shareLinks");
    public IMongoCollection<AuditLog> AuditLogs => _database.GetCollection<AuditLog>("auditLogs");
    public IMongoCollection<UploadSession> UploadSessions => _database.GetCollection<UploadSession>("uploadSessions");
}
