using DocumentFormat.OpenXml.Packaging;
using UglyToad.PdfPig;
using System.IO;
using System.IO.Compression;
using Word = Microsoft.Office.Interop.Word;
using GroupDocs.Parser;


namespace FolderAnalyzer.Services;


public class TextExtractorService
{
    public string ExtractText(string filePath)
    {
        string extension = Path.GetExtension(filePath)
                               .ToLowerInvariant();
        if (extension == ".doc")
        {
            Console.WriteLine($"Extraction du texte du fichier DOC : {filePath}");
        }

        return extension switch
        {
            ".txt" => ReadText(filePath),
            ".csv" => ReadText(filePath),
            ".json" => ReadText(filePath),
            ".xml" => ReadText(filePath),
            ".docx" => ReadDocx(filePath),
            ".pdf" => ReadPdf(filePath),
            ".odt" => ReadOdt(filePath),
            ".doc" => ReadDoc(filePath),
            _ => string.Empty
        };
    }

    private string ReadText(string path)
    {
        return File.ReadAllText(path);
    }

    private string ReadDocx(string path)
    {
        Console.WriteLine($"Lecture du fichier DOCX : {path}");

        using var document =
            WordprocessingDocument.Open(path, false);

        return document.MainDocumentPart?
            .Document?
            .Body?
            .InnerText ?? string.Empty;
    }

    private string ReadPdf(string path)
    {
        using PdfDocument document =
            PdfDocument.Open(path);

        return string.Join(
            Environment.NewLine,
            document.GetPages()
                    .Select(p => p.Text));
    }
    private string ReadOdt(string path)
    {
        using var archive = ZipFile.OpenRead(path);

        var entry = archive.GetEntry("content.xml");

        if (entry == null)
            return string.Empty;

        using var stream = entry.Open();
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
    private string ReadDoc(string path)
    {
            Console.WriteLine($"Lecture du fichier DOC : {path}");

        using (Parser parser = new Parser(path))
        {
            using (TextReader textReader = parser.GetText())
            {
                return textReader.ReadToEnd();
            }
        }
    }
}