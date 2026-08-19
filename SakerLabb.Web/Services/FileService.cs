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
        var path = Path.Combine(_root, name);
        _logger.LogInformation("Läser bilaga {Name} från {Path}", name, path);
        return File.ReadAllText(path);
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
