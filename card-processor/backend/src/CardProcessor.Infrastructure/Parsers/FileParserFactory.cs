using CardProcessor.Core;

namespace CardProcessor.Infrastructure.Parsers;

public class FileParserFactory : IFileParserFactory
{
    private readonly IEnumerable<IFileParser> _parsers;

    public FileParserFactory()
    {
        _parsers = new List<IFileParser>
        {
            new CsvFileParser(),
            new JsonFileParser(),
            new XmlFileParser()
        };
    }

    public IFileParser CreateParser(string filePath)
    {
        var parser = _parsers.FirstOrDefault(p => p.CanParseFile(filePath));
        
        if (parser == null)
        {
            var extension = Path.GetExtension(filePath);
            throw new NotSupportedException($"No parser found for file extension: {extension}");
        }

        return parser;
    }
}


