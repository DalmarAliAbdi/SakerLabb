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
        // 1. Avvisa direkt om filnamnet är tomt eller innehåller sökvägsmanipulation
        if (string.IsNullOrWhiteSpace(name) || name.Contains("..") || name.Contains('/') || name.Contains('\\'))
        {
            throw new ArgumentException("Ogiltigt filnamn.");
        }

        // 2. Säkerställ korrekt rotsökväg med katalogseparator
        var rootPath = Path.GetFullPath(_root);
        if (!rootPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
            rootPath += Path.DirectorySeparatorChar;
        }

        var safeFileName = Path.GetFileName(name);
        var fullPath = Path.GetFullPath(Path.Combine(rootPath, safeFileName));

        // 3. Verifiera att sökvägen strikt stannar inom rotkatalogen
        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Åtkomst nekad.");
        }

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
