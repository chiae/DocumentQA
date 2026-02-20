using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using System.Text;


namespace DocumentQA.Services
{
   public class PdfTextExtractor : IPdfTextExtractor
    {
        public string ExtractText(Stream pdfStream)
        {
            using var document = PdfDocument.Open(pdfStream);
            var builder = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                builder.AppendLine(page.Text);
            }

            return builder.ToString();
        }
    }
}
