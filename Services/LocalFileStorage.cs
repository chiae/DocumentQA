namespace DocumentQA.Services
{
    public class LocalFileStorage : IFileStorage
    {
        private readonly string _filePath;

        public LocalFileStorage(string path)
        {
            _filePath = path;
        }

        public async Task<string> SaveAsync(string documentId, Stream fileStream)
        {
            Directory.CreateDirectory(_filePath);

            var filePath = Path.Combine(_filePath, $"{documentId}.pdf");

            using var output = File.Create(filePath);
            await fileStream.CopyToAsync(output);

            return filePath;
        }

        public Task<Stream> OpenReadAsync(string documentId)
        {
            var filePath = Path.Combine(_filePath, $"{documentId}.pdf");

            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            return Task.FromResult<Stream>(File.OpenRead(filePath));
        }
    }
}