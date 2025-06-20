using System.Text.RegularExpressions;

namespace Bot.BLL.DocumentProcessing;

public class DocumentParser
{
    public (string FullName, string VIN) ParseData(string passportText, string vehicleDocText)
    {
        var fullName = ParseFullName(passportText);
        var vin = ParseVin(vehicleDocText);
        return (fullName, vin);
    }

    private string ParseFullName(string text)
    {
        var regex = new Regex(@"P<\s*(.+?)<<", RegexOptions.IgnoreCase);

        var match = regex.Match(text);
        if (match.Success)
        {
            var result = match.Groups[1].Value;
            result = Regex.Replace(result, @"[<\s]+", " ").Trim();
            return result;
        }
        return "Unknown Name";
    }


    private string ParseVin(string text)
    {
        var vinKeywords = new[] { "VIN", "ідентифікаційний номер", "ідентифікаційний номер КТЗ" };
        foreach (var key in vinKeywords)
        {
            var pattern = $@"{key}[^A-Z0-9]*([A-HJ-NPR-Z0-9]{{16,17}})";
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value;
        }
        
        var fallbackRegex = new Regex(@"\b([A-HJ-NPR-Z0-9]{16,17})\b");
        var fallbackMatch = fallbackRegex.Match(text);
        if (fallbackMatch.Success)
            return fallbackMatch.Value;

        return "Unknown VIN";
    }

}