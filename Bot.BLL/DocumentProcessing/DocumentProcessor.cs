using System;
using System.IO;
using Tesseract;

namespace Bot.BLL.DocumentProcessing;

public class DocumentProcessor
{
    public string ProcessDocuments(Stream imageStream)
    {
        try
        {
            var tessDataPath = Path.Combine(AppContext.BaseDirectory, "tessdata");
            using var engine = new TesseractEngine(tessDataPath, "eng+ukr", EngineMode.Default);

            using var img = Pix.LoadFromMemory(ReadStream(imageStream));
            using var page = engine.Process(img);
            return page.GetText().Replace("\n", " ").Replace("\r", "").Trim();
        }
        catch (Exception ex)
        {
            return $"Error OCR: {ex.Message}";
        }
    }

    private byte[] ReadStream(Stream input)
    {
        using var ms = new MemoryStream();
        input.CopyTo(ms);
        return ms.ToArray();
    }
}