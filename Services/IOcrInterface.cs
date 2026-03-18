namespace DocumentQA.Services
{
    public interface IOcrService
    {
        Task<string> ExtractTextAsync(Stream pdfStream);
    }
}
