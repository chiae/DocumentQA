using DocumentQA.Data;
using DocumentQA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DocumentQA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly VectorDbContext _db;

        public DocumentsController(VectorDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentEntity>>> GetDocuments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var docs = await _db.Documents
                .Where(d => d.UserId == userId)
                .OrderBy(d => d.UploadedAt)
                .Select( d => new
                {
                    d.Id,
                    d.FileName,
                    d.Description,
                    d.UploadedAt,
                    d.PageCount,
                    d.ChunkCount
                })
        .ToListAsync();


            return Ok(docs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetDocumentById(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var doc = await _db.Documents
                .Where(d => d.UserId == userId && d.Id == id)
                .Select(d => new
                {
                    d.Id,
                    d.FileName,
                    d.Description,
                    d.UploadedAt,
                    d.PageCount,
                    d.ChunkCount
                })
                .FirstOrDefaultAsync();

            if (doc == null)
                return NotFound();

            return Ok(doc);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var doc = await _db.Documents
                .Where(d => d.Id == id && d.UserId == userId)
                .FirstOrDefaultAsync();

            if (doc == null)
                return NotFound();

            // Delete document record
            _db.Documents.Remove(doc);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}