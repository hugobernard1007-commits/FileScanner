using FolderAnalyzer.Models;
using System.Text;
using System.IO;

namespace FolderAnalyzer.Services;

public class CsvExportService
{
    public void Export(
        IEnumerable<FileItem> files,
        string path)
    {
        var sb = new StringBuilder();

        sb.AppendLine(
            "Nom;Extension;Taille;Création;Modification;Chemin");

        foreach (var file in files)
        {
            sb.AppendLine(
                $"{file.Name};" +
                $"{file.Extension};" +
                $"{file.Size};" +
                $"{file.CreationDate};" +
                $"{file.LastModifiedDate};" +
                $"{file.FullPath}");
        }

        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }
}