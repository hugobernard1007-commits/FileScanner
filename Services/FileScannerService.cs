using FolderAnalyzer.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace FolderAnalyzer.Services;

public class FileScannerService
{
    private readonly ConfidentialityAnalyzer analyzer = new();
    private readonly TextExtractorService extractor = new();
    public async Task ScanAsync(
                                string folder,
                                Action<FileItem> addFile,
                                IProgress<ScanResult>? progress = null,
                                CancellationToken cancellationToken = default)
    {
        await Task.Run(async () =>
        {

           try
            {
                var files = GetFilesSafe(folder).ToList();

                Console.WriteLine($"Fichiers trouvés : {files.Count}");
            
                int totalFiles = files.Count;

                
                int current = 0;

                foreach (var file in files)
                {
                    int score = 0;
                    string level = "Public";

                    try
                    {
                        string content = extractor.ExtractText(file);

                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            var analysis = await analyzer.AnalyzeAsync(content);

                            score = analysis.Score;
                            level = analysis.Level;
                        }
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText("errors.log", $"{file} : {ex}\n");
                    }
                    cancellationToken.ThrowIfCancellationRequested();

                    current++;

                    try
                    {
                        var info = new FileInfo(file);

                        var fileItem = new FileItem
                        {
                            Name = info.Name,
                            Extension = info.Extension,
                            Size = info.Length,
                            CreationDate = info.CreationTime,
                            LastModifiedDate = info.LastWriteTime,
                            FullPath = info.FullName,
                            Attributes = info.Attributes.ToString(),
                            ConfidentialityScore = score,
                            ConfidentialityLevel = level
                        };
                            
                        addFile(fileItem);

                        int percentage = totalFiles == 0
                        ? 100
                        : (int)((double)current / totalFiles * 100);

                            progress?.Report(new ScanResult
                            {
                                File = fileItem,
                                Percentage = percentage,
                                FileName = Path.GetFileName(file)
                            });

                    }
                    catch(Exception ex)
                    {
                        File.AppendAllText("errors.log", $"{file} : {ex}\n");
                    }
                    

                }
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
        });
    }
    private static bool IsTextFile(string extension)
    {
        string[] extensions =
        {
            ".txt",
            ".csv",
            ".log",
            ".json",
            ".xml",
            ".config",
            ".cs",
            ".sql",
            ".html",
            ".md"
        };

        return extensions.Contains(
            extension,
            StringComparer.OrdinalIgnoreCase);
    }

    private IEnumerable<string> GetFilesSafe(string root)
    {
        var pending = new Stack<string>();
        pending.Push(root);

        while (pending.Count > 0)
        {
            string path = pending.Pop();

            string[]? subDirectories = null;

            try
            {
                subDirectories = Directory.GetDirectories(path);
            }
            catch(Exception ex)
            {
                File.AppendAllText("errors.log", $"{path} : {ex}\n");
                continue;
            }

            foreach (var directory in subDirectories)
            {
                 try
                {
                    var attributes = File.GetAttributes(directory);

                    // Ignore les points de montage / liens symboliques
                    if ((attributes & FileAttributes.ReparsePoint)
                        == FileAttributes.ReparsePoint)
                    {
                        continue;
                    }

                    // Ignore les dossiers système
                    if ((attributes & FileAttributes.System)
                        == FileAttributes.System)
                    {
                        continue;
                    }

                    pending.Push(directory);
                }
                catch(Exception ex)
                {
                    File.AppendAllText("errors.log", $"{directory} : {ex}\n");
                    continue;
                }
            }

            string[]? files = null;

            try
            {
                files = Directory.GetFiles(path);
            }
            catch(Exception ex)
            {
                File.AppendAllText("errors.log", $"{path} : {ex}\n");
                continue;
            }

            foreach (var file in files)
            {
                yield return file;
            }
        }
    }

    public class ScanProgress
    {
        public FileItem File { get; set; } = null!;
        public int Percentage { get; set; }
        public string FileName { get; set; } = "";
    }

    public class ScanResult
    {
        public FileItem File { get; set; } = null!;
        public int Percentage { get; set; }
        public string FileName { get; set; } = "";
    }
}