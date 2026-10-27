namespace FolderAnalyzer.Models;

public class FileItem
{
    public string Name { get; set; } = "";
    public string Extension { get; set; } = "";
    public long Size { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public string FullPath { get; set; } = "";
    public string Attributes { get; set; } = "";

    public int ConfidentialityScore { get; set; }

    public string ConfidentialityLevel { get; set; } = "";
}