namespace SakerLabb.Web.Services;

public class FileService
{
    private readonly string _root;
    private readonly ILogger<FileService> _logger;

    public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
    {
        _root = Path.Combine(environment.WebRootPath, "files");
        _logger = logger;
        Directory.CreateDirectory(_root);
    }

    public string ReadDocument(string name)
    {
    var safeFileName = Path.GetFileName(name);
    var rootPath = Path.GetFullPath(_root);
    var fullPath = Path.GetFullPath(Path.Combine(rootPath, safeFileName));

    if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
    {
        _logger.LogWarning("Försök till Path Traversal upptäckt för fil: {Name}", name);
        throw new UnauthorizedAccessException("Ogiltig eller otillåten filsökväg.");
    }

    _logger.LogInformation("Läser bilaga {Name} från {Path}", safeFileName, fullPath);
    return File.ReadAllText(fullPath);
    }

    public byte[] ReadBytes(string name)
    {
        return File.ReadAllBytes(Path.Combine(_root, name));
    }

    public IEnumerable<string> List()
    {
        return Directory.EnumerateFiles(_root).Select(Path.GetFileName).OfType<string>();
    }

    public async Task<string> SaveUpload(IFormFile file)
    {
        var target = Path.Combine(_root, file.FileName);
        await using var stream = File.Create(target);
        await file.CopyToAsync(stream);
        _logger.LogInformation("Bilaga sparad som {Target}", target);
        return file.FileName;
    }

    public void Delete(string name)
    {
        File.Delete(Path.Combine(_root, name));
    }
}
