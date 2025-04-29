using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[Produces("application/json")]
[Consumes("application/json", "multipart/form-data")]
[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly BookService _bookService;

    public BooksController(BookService bookService)
    {
        _bookService = bookService;
    }

    //[Authorize(Roles = "User")]
    [HttpGet]
    public ActionResult<List<Book>> Get() => _bookService.Get();

    [HttpGet("{id:length(24)}", Name = "GetBook")]
    public ActionResult<Book> Get(string id)
    {
        var book = _bookService.GetOneBook(id);

        if (book == null)
        {
            return NotFound();
        }

        return book;
    }
    public async Task<Book> Create([FromForm(Name = "icon")] IFormFile file, [FromForm(Name = "body")] string body)
    {
        Book book = JsonConvert.DeserializeObject<Book>(body);

        return await _bookService.Create(book, file);
    }

    [HttpPut("{id:length(24)}")]
    public IActionResult Update(string id, Book bookIn)
    {
        var book = _bookService.GetOneBook(id);

        if (book == null)
        {
            return NotFound();
        }

        _bookService.Update(id, bookIn);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public IActionResult Delete(string id)
    {
        var book = _bookService.GetOneBook(id);

        if (book == null)
        {
            return NotFound();
        }

        _bookService.Remove(book, book.IconId);

        return NoContent();
    }
}