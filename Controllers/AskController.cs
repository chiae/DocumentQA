using DocumentQA.Data;
using DocumentQA.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DocumentQA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class AskController : ControllerBase
    {
        private readonly IRagService _rag;
        private readonly ILogger<AskController> _logger;
        private readonly VectorDbContext _db;

        public AskController(IRagService rag, ILogger<AskController> logger, VectorDbContext db)
        {
            _rag = rag;
            _logger = logger;
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // CASE 1: DocumentId provided → enforce ownership
                if (!string.IsNullOrWhiteSpace(request.DocumentId))
                {
                    var document = await _db.Documents
                        .AsNoTracking()
                        .FirstOrDefaultAsync(d => d.Id == request.DocumentId);

                    if (document == null)
                        return NotFound("Document not found.");

                    if (document.UserId != userId)
                        return Forbid();

                    var answer = await _rag.AskAsync(request.Question, request.DocumentId, userId);
                    return Ok(new { answer });
                }

                // CASE 2: No DocumentId → search all documents uploaded by this user
                var answerAll = await _rag.AskAsync(request.Question, null, userId);
                return Ok(new { answer = answerAll });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ASK ERROR");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class AskRequest
    {
        public string Question { get; set; } = string.Empty;
        public string? DocumentId { get; set; }
    }
}