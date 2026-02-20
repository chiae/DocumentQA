using DocumentQA.Data;
using DocumentQA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocumentQA.Controllers
{
    [ApiController]
    [Route("api/test-db")]
    public class TestDbController : ControllerBase
    {
        private readonly VectorDbContext _context;

        public TestDbController(VectorDbContext context)
        {
            _context = context;
        }

        [HttpPost("insert")]
        public async Task<IActionResult> InsertTestChunk()
        {
            var chunk = new ChunkEntity
            {
                DocumentId = "test-doc",
                Index = 0,
                Text = "Hello from the test endpoint",
                Embedding = new float[] { 0.1f, 0.2f, 0.3f }
            };

            _context.Chunks.Add(chunk);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Inserted", chunk.Id });
        }

        [HttpGet("read")]
        public async Task<IActionResult> ReadTestChunks()
        {
            var chunks = await _context.Chunks
                .Where(c => c.DocumentId == "test-doc")
                .ToListAsync();

            return Ok(chunks);
        }
    }
}
