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
        text = text.Replace("\n", " ").Replace("\r", " ");

        var regex = new Regex(@"\b([A-ZА-ЯІЇЄҐ]{2,})\s+([A-ZА-ЯІЇЄҐ]{2,})\b", RegexOptions.IgnoreCase);
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