

public interface Token
{
    public string Key { get; set; }
    public string Issuer { get; set; }
}

public interface IDatabaseSettings
{
    public string BooksCollectionName { get; set; }

    public string ConnectionString { get; set; }

    public string DatabaseName { get; set; }

    public Token Token { get; set; }
}

public class DatabaseSettings : IDatabaseSettings
{
    public string BooksCollectionName { get; set; }

    public string ConnectionString { get; set; }

    public string DatabaseName { get; set; }

    public Token Token { get; set; }
}