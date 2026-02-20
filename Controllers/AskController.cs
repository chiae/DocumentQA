using DocumentQA.Services;
using Microsoft.AspNetCore.Mvc;

namespace DocumentQA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AskController : ControllerBase
    {
        private readonly IRagService _rag;
        private readonly ILogger<AskController> _logger;


        public AskController(IRagService rag, ILogger<AskController> logger)
        {
            _rag = rag;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            try
            {
                var answer = await _rag.AskAsync(request.Question, request.DocumentId);
                return Ok(new { answer });
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