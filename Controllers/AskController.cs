using DocumentQA.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocumentQA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AskController : ControllerBase
    {
        private readonly IRagService _rag;

        public AskController(IRagService rag)
        {
            _rag = rag;
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            var answer = await _rag.AskAsync(request.Question, request.DocumentId);
            return Ok(new { answer });
        }
    }



    public class AskRequest
    {
        public string Question { get; set; } = string.Empty;
        public string? DocumentId { get; set; }

    }
}