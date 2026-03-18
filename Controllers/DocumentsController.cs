using DocumentQA.Data;
using DocumentQA.Models;
using DocumentQA.Services;
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
        private readonly IFileStorage _fileStorage;

        public DocumentsController(VectorDbContext db,IFileStorage fileStorage)
        {
            _db = db;
            _fileStorage = fileStorage;
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
        [HttpGet("{id}/file")]
        public async Task<IActionResult> GetDocumentFile(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Ensure the document belongs to the user
            var doc = await _db.Documents
                .Where(d => d.Id == id && d.UserId == userId)
                .FirstOrDefaultAsync();

            if (doc == null)
                return NotFound(new { error = "Document not found." });

            try
            {
                // Open the PDF stream from storage
                var stream = await _fileStorage.OpenReadAsync(id);

                return File(stream, "application/pdf", doc.FileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound(new { error = "File missing from storage." });
            }
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