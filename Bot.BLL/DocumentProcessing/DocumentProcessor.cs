using System.Globalization;
using Mindee;
using Mindee.Input;
using Mindee.Http;
using Mindee.Product.Passport;
using Mindee.Product.Generated;

namespace Bot.BLL.DocumentProcessing;

public class DocumentProcessor(string apiKey)
{
    private readonly MindeeClient _mindeeClient = new(apiKey);

    private string CleanOutput(string rawText)
    {
        var lines = rawText.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var cleanedLines = lines
            .Select(line =>
            {
                var cleaned = line.TrimStart(':');
                cleaned = cleaned.Replace(":value:", "").Trim();
                return cleaned;
            });
    
        return string.Join('\n', cleanedLines);
    }

    public async Task<string> ProcessPassportAsync(Stream imageStream)
    {
        string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");

        try
        {
            await using (var fileStream = File.Create(tempFilePath))
            {
                await imageStream.CopyToAsync(fileStream);
            }

            var input = new LocalInputSource(tempFilePath);

            var response = await _mindeeClient.ParseAsync<PassportV1>(input);

            var prediction = response.Document?.Inference?.Prediction;
            return CleanOutput(prediction?.ToString() ?? "Error: no passport data");
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    public async Task<string> ProcessVehicleDocAsync(Stream imageStream)
    {
        var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".png");

        try
        {
            await using (var fileStream = File.Create(tempFilePath))
            {
                await imageStream.CopyToAsync(fileStream);
            }
            
            var input = new LocalInputSource(tempFilePath);

            var customEndpoint = new CustomEndpoint(
                endpointName: "vehicle_identification_document",
                accountName: "yukinon",
                version: "1"
            );

            var response = await _mindeeClient.EnqueueAndParseAsync<GeneratedV1>(input, customEndpoint);

            var prediction = response.Document?.Inference?.Prediction;
            return CleanOutput(prediction?.ToString() ?? "Error: no vehicle data");
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }
}

