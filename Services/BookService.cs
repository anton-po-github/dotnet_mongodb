using MongoDB.Driver;

public class BookService
{
    private readonly DataContext _dataContext = null;
    private readonly FileService _fileService;

    public BookService(IDatabaseSettings databaseSettings, FileService fileService)
    {
        _dataContext = new DataContext(databaseSettings);
        _fileService = fileService;
    }

    public List<Book> Get() =>
        _dataContext.Books.Find(book => true).ToList();

    public Book GetOneBook(string id) =>
        _dataContext.Books.Find<Book>(book => book.Id == id).FirstOrDefault();

    public async Task<Book> Create(Book book, IFormFile file)
    {
        var objectId = await _fileService.UploadFile(file);

        book.IconId = objectId.ToString();

        book.IconPath = await _fileService.GetBytesByName(file.FileName);

        _dataContext.Books.InsertOne(book);

        return book;
    }

    public void Update(string id, Book bookIn) =>
        _dataContext.Books.ReplaceOne(book => book.Id == id, bookIn);

    public void Remove(Book bookIn, string iconId)
    {
        _dataContext.Books.DeleteOne(book => book.Id == bookIn.Id);

        _fileService.DeleteFile(iconId);
    }

    public void Remove(string id) =>
        _dataContext.Books.DeleteOne(book => book.Id == id);
}
