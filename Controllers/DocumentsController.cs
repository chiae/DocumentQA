using DocumentQA.Data;
using DocumentQA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace DocumentQA.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        public readonly VectorDbContext _db;
        public DocumentsController(VectorDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentEntity>>> GetDocuments()
        {
            var docs = await _db.Documents.OrderBy(d => d.UploadedAt).ToListAsync();
            return Ok(docs);
        }

    }
}
