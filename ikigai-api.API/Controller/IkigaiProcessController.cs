
using ikigai_api.Application.DTOs;
using ikigai_api.Application.Interfaces;
using ikigai_api.Common.Extensions;
using ikigai_api.Domain.Entities;
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

        [HttpGet("result/{userId}")]
        public async Task<IActionResult> GetResult(Guid userId)
        {
            var result = await _ikigaiService.GetIkigaiResultAsync(userId);
            if (result == null) return NotFound(new { message = "Result not found" });

            return Ok(result);
        }

        [HttpPost("generate/{userId}")]
        public async Task<IActionResult> GenerateIkigai(Guid userId)
        {
            var result = await _ikigaiService.StartIkigaiProcessingAsync(userId);

            //? Case 1: ถ้าเป็นงานที่เสร็จอยู่แล้ว (Existing Completed)
            if (result.IsExisting || result.Status == ProcessStatus.Completed)
            {
                // Return 200 OK พร้อมบอก Frontend ว่าเสร็จแล้วนะ ไปดึงข้อมูลได้เลย
                return Ok(new
                {
                    processId = result.ProcessId,
                    status = result.Status.ToString()
                });
            }

            //? Case 2: ถ้าเพิ่งเริ่มทำ (New Job)
            // Return 202 Accepted (รับเรื่องไว้แล้ว กำลังทำ)
            return Accepted(new
            {
                processId = result.ProcessId,
                status = result.Status.ToString()
            });
        }

        [HttpGet("status/{processId}")]
        public async Task<IActionResult> GetStatus(Guid processId)
        {
            var result = await _ikigaiService.GetProcessStatusAsync(processId);
            if (result == null) return NotFound();

            if (result.Status == ProcessStatus.Completed)
            {
                var response = new IkigaiResultDto
                {
                    Id = result.Id,
                    Status = result.Status.ToString(),
                    Summaries = result.IkigaiSummaries.Select(s => new IkigaiSummaryDto
                    {
                        ComponentType = s.ComponentType,
                        OverallSummary = s.OverallSummary,
                        ShortSummary = s.ShortSummary,
                        Strengths = s.StrengthsJson.FromJsonThai<List<string>>(),
                        DevelopmentPoints = s.DevelopmentPointsJson.FromJsonThai<List<string>>()

                    }).ToList()
                };
                return Ok(response);
            }
            else if (result.Status == ProcessStatus.Failed)
            {
                return BadRequest(new { error = result.ErrorMessage });
            }
            else
            {
                return Ok(new { status = result.Status.ToString() });
            }
        }
    }
}