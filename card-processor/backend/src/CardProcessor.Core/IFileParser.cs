namespace CardProcessor.Core;

public interface IFileParser
{
    Task<IEnumerable<Transaction>> ParseFileAsync(string filePath);
    bool CanParseFile(string filePath);
    string GetFileExtension();
}

public interface IFileParserFactory
{
    IFileParser CreateParser(string filePath);
}


