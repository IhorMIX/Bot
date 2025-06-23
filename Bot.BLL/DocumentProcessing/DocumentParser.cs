using System.Text.RegularExpressions;

namespace Bot.BLL.DocumentProcessing;

public class DocumentParser
{
    public (string FullName, string VIN) ParseData(string passportText, string vehicleDocText)
    {
        var fullName = ParseFullName(passportText);
        var vin = ParseVIN(vehicleDocText);
        return (fullName, vin);
    }

    private string ParseFullName(string text)
    {
        text = text.Replace("\r", "").Trim();
        
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        string? givenName = null;
        string? surname = null;

        foreach (var line in lines)
        {
            if (line.StartsWith("Given Name", StringComparison.OrdinalIgnoreCase))
            {
                givenName = line.Split(':').Last().Trim();
            }
            else if (line.StartsWith("Surname", StringComparison.OrdinalIgnoreCase))
            {
                surname = line.Split(':').Last().Trim();
            }
        }

        if (!string.IsNullOrEmpty(givenName) && !string.IsNullOrEmpty(surname))
            return $"{givenName} {surname}";
        
        var regex = new Regex(@"\b([A-ZА-ЯІЇЄҐ][a-zа-яіїєґ']{1,})\s+([A-ZА-ЯІЇЄҐ][a-zа-яіїєґ']{1,})\b", RegexOptions.IgnoreCase);
        var match = regex.Match(text);
        if (match.Success)
            return match.Value.Trim();

        return "Unknown Name";
    }

    private string ParseVIN(string text)
    {
        var regex = new Regex(@"\b([A-HJ-NPR-Z0-9]{17})\b");
        var match = regex.Match(text);
        if (match.Success)
            return match.Value.Trim();

        return "Unknown VIN";
    }
}