using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;

[Produces("application/json")]
[Route("api/[controller]")]
public class UploadController : Controller
{
    private readonly IHostEnvironment _hostEnvironment;

    private readonly UploadFile _uploadFile = new UploadFile();
    public UploadController(IHostEnvironment hostingEnvironment)
    {
        _hostEnvironment = hostingEnvironment;
    }

    /*
    [HttpPost("api/file")]
    public ActionResult Student([FromForm]UploadFile file)
    {
        // Getting Name
        string name = file.Name;
        Console.WriteLine("name -> " + name);

        // Getting Image
        var image = file.Image;
        Console.WriteLine("image -> " + image);
        // Saving Image on Server
        if (image.Length > 0) {
            using (var fileStream = new FileStream(image.FileName, FileMode.Create)) {
                image.CopyTo(fileStream);
            }
        }

        return Ok(new { status = true, message = "Student Posted Successfully"});
    }*/

    /*public string UploadFile(IFormFile fileFromUI)
    {
        try
        {
            var file = fileFromUI;
            string folderName = "Upload";
            string webRootPath = _hostEnvironment.ContentRootPath;
            string newPath = Path.Combine(webRootPath, folderName);
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            if (file.Length > 0)
            {
                string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string fullPath = Path.Combine(newPath, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }
            string pathImage = (ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"'));
            _usersItemsController.ImagePath = (ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"'));
            return pathImage;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }*/

    [HttpPost, DisableRequestSizeLimit]
    public ActionResult UploadFileRequest()
    {
        try
        {
            var file = Request.Form.Files[0];
            string folderName = "Uploads";
            string webRootPath = _hostEnvironment.ContentRootPath;
            string newPath = Path.Combine(webRootPath, folderName);

            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            if (file.Length > 0)
            {
                string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string fullPath = Path.Combine(newPath, fileName);
                _uploadFile.WriteImagePath(fullPath);
                Console.WriteLine(_uploadFile.ImagePath);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }
            return Json("Upload Successful. " + newPath + "/" + file);
        }
        catch (System.Exception ex)
        {
            return Json("Upload Failed: " + ex.Message);
        }
    }

    // GET api/<controller>/5
    [HttpGet("{id}")]
    public IActionResult GetImageUpload(string id)
    {
        var path = Path.Combine(_hostEnvironment.ContentRootPath, "Uploads", $"{id}.png");
        var imageFileStream = System.IO.File.OpenRead(path);
        return File(imageFileStream, "image/jpeg");
    }

}