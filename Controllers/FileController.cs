using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

// [Authorize]
[ApiController]
[Route("[controller]")]
public class FileController : ControllerBase
{
    private readonly FileService _fileService;

    public FileController(FileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost]
    public async Task<string> Upload([FromForm(Name = "icon")] IFormFile file)
    {
        ObjectId id = await _fileService.UploadFile(file);
        return id.ToString();
    }

    [HttpGet("test")]
    public async Task<string> DownloadFile()
    {
        await _fileService.DownloadFileMongo();
        return "";
    }

    [HttpPost("uploadFile")]
    public async Task<ObjectId> UploadFile(IFormFile file)
    {
        return await _fileService.UploadFile(file);
    }

    [HttpGet("getFileInfo/{id}")]
    public Task<String> GetFileInfo(string id)
    {
        return _fileService.GetFileInfo(id);
    }
}
