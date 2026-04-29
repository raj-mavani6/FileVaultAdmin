namespace FileVaultAdmin.Helpers;

public static class FormatHelpers
{
    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    public static string TimeAgo(DateTime dt)
    {
        var span = DateTime.UtcNow - dt;
        if (span.TotalMinutes < 1) return "just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
        if (span.TotalDays < 30) return $"{(int)span.TotalDays}d ago";
        return dt.ToString("MMM dd, yyyy");
    }

    public static string GetFileIcon(string extension)
    {
        return extension?.ToLowerInvariant() switch
        {
            ".pdf" => "bi-file-earmark-pdf-fill",
            ".doc" or ".docx" => "bi-file-earmark-word-fill",
            ".xls" or ".xlsx" => "bi-file-earmark-excel-fill",
            ".ppt" or ".pptx" => "bi-file-earmark-ppt-fill",
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" or ".svg" => "bi-file-earmark-image-fill",
            ".mp4" or ".avi" or ".mkv" or ".mov" or ".wmv" => "bi-file-earmark-play-fill",
            ".mp3" or ".wav" or ".ogg" or ".flac" => "bi-file-earmark-music-fill",
            ".zip" or ".rar" or ".7z" or ".tar" or ".gz" => "bi-file-earmark-zip-fill",
            ".cs" or ".js" or ".ts" or ".py" or ".java" or ".cpp" or ".html" or ".css" => "bi-file-earmark-code-fill",
            ".txt" or ".md" or ".log" => "bi-file-earmark-text-fill",
            ".json" or ".xml" or ".yaml" or ".yml" => "bi-file-earmark-code-fill",
            _ => "bi-file-earmark-fill"
        };
    }
}
