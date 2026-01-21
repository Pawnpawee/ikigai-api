using ikigai_api.Application.DTOs;
using ikigai_api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MyThesis.API.Controllers
{
    [ApiController]
    [Route("api/user/progress")]
    public class UserProgressController : ControllerBase
    {
        private readonly IIkigaiService _ikigaiService;

        public UserProgressController(IIkigaiService ikigaiService)
        {
            _ikigaiService = ikigaiService;
        }

        [HttpPost("prologue")]
        public async Task<IActionResult> SavePrologue([FromBody] SavePrologueRequest request)
        {
            if (string.IsNullOrEmpty(request.PlayerName))
            {
                return BadRequest(new { error = "Player name is required." });
            }

            var userId = await _ikigaiService.SavePrologueAsync(request);

            return Ok(new
            {
                message = "Prologue progress saved successfully.",
                userId = userId
            });
        }

        [HttpPost("love")]
        public async Task<IActionResult> SaveLoveSession([FromBody] SaveLoveSessionRequest request)
        {
            try
            {
                await _ikigaiService.SaveLoveSessionAsync(request);
                return Ok(new { message = "Love session progress saved successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}