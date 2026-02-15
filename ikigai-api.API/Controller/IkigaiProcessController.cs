using ikigai_api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ikigai_api.API.Controllers
{
    [ApiController]
    [Route("api/ikigai")]
    public class IkigaiProcessController : ControllerBase
    {
        private readonly IIkigaiService _ikigaiService;

        public IkigaiProcessController(IIkigaiService ikigaiService)
        {
            _ikigaiService = ikigaiService;
        }

        [HttpPost("process/{userId}")]
        public async Task<IActionResult> ProcessIkigai(Guid userId)
        {
            try
            {
                var result = await _ikigaiService.ProcessIkigaiAsync(userId);
                return Ok(new { message = "Processing completed", resultId = result.Id });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal Server Error", details = ex.Message });
            }
        }

        [HttpGet("result/{userId}")]
        public async Task<IActionResult> GetResult(Guid userId)
        {
            var result = await _ikigaiService.GetIkigaiResultAsync(userId);
            if (result == null) return NotFound(new { message = "Result not found" });

            return Ok(result);
        }
    }
}