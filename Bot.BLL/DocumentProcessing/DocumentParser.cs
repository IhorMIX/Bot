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
        var surnameMatch = Regex.Match(text, @"(Surname|Last Name):\s*(.+)", RegexOptions.IgnoreCase);
        if (!surnameMatch.Success)
        {
            surnameMatch = Regex.Match(text, @"Name:\s*(.+)", RegexOptions.IgnoreCase);
        }
        
        var givenNameMatch = Regex.Match(text, @"Given Name\(s\):\s*(.+)", RegexOptions.IgnoreCase);
        if (!givenNameMatch.Success)
        {
            givenNameMatch = Regex.Match(text, @"Name:\s*(.+)", RegexOptions.IgnoreCase);
        }

        string surname = surnameMatch.Success ? surnameMatch.Groups[2].Value.Trim() : null;
        string givenName = givenNameMatch.Success ? givenNameMatch.Groups[1].Value.Trim() : null;
        
        if (!string.IsNullOrEmpty(givenName) && !string.IsNullOrEmpty(surname))
        {
            return $"{givenName} {surname}";
        }
        
        if (!string.IsNullOrEmpty(surname))
        {
            return surname;
        }
        
        if (!string.IsNullOrEmpty(givenName))
        {
            return givenName;
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