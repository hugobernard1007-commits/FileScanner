using System.Net.Http;
using System.Net.Http.Json;


namespace FolderAnalyzer.Services;
public class PresidioService
{
    private readonly HttpClient _client = new();

    public async Task<List<PresidioEntity>> AnalyzeAsync(string text)
    {
        var response = await _client.PostAsJsonAsync(
            "http://localhost:5000/analyze",
            new { text });

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<PresidioEntity>>()
            ?? [];
    }
}

public class PresidioEntity
{
    public string Entity { get; set; } = "";
    public double Score { get; set; }
    public int Start { get; set; }
    public int End { get; set; }
}