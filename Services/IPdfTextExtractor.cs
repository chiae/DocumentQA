namespace DocumentQA.Services
{
    public interface IPdfTextExtractor
    {
        string ExtractText(Stream pdfStream);
    }
}
