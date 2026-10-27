using System.Text.RegularExpressions;

namespace FolderAnalyzer.Services;

public class ConfidentialityAnalyzer
{
    private readonly PresidioService _presidioService;

    public ConfidentialityAnalyzer()
    {
        _presidioService = new PresidioService();
    }
    private readonly Dictionary<string, int> keywords =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["confidentiel"] = 10,
            ["secret"] = 15,
            ["interne"] = 5,
            ["rgpd"] = 10,
            ["mot de passe"] = 20,
            ["password"] = 20,
            ["iban"] = 15,
            ["salaire"] = 10,
            ["contrat"] = 5,
            ["client"] = 3,
            ["pas diffuser"] = 20
        };

    public async Task<(int Score, string Level)> AnalyzeAsync(string content)
    {

        const int MaxChars = 10000;
        string text = content;

        if(content.Length > MaxChars)
        {
            text = content[..MaxChars];
        }
        var presidioResults = await _presidioService.AnalyzeAsync(text);

        var score = 0;

        foreach (var keyword in keywords)
        {
            int count = Regex.Matches(
                content,
                Regex.Escape(keyword.Key),
                RegexOptions.IgnoreCase)
                .Count;

            score += count * keyword.Value;
        }

        string level = score switch
        {
            0 => "Public",
            <= 30 => "Interne",
            <= 60 => "Sensible",
            _ => "Très confidentiel"
        };

        foreach (var entity in presidioResults)
        {
            switch (entity.Entity)
            {
                case "EMAIL_ADDRESS":
                    score += 5;
                    break;

                case "PHONE_NUMBER":
                    score += 5;
                    break;

                case "PERSON":
                    score += 2;
                    break;

                case "CREDIT_CARD":
                    score += 50;
                    break;

                default:
                    score += 1;
                    break;
            }
        }
        
        return (score, level);
    }
}