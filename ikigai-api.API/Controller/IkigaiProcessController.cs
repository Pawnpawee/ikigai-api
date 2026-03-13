
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
        private readonly ISseManager _sseManager;

        public IkigaiProcessController(IIkigaiService ikigaiService, ISseManager sseManager)
        {
            _ikigaiService = ikigaiService;
            _sseManager = sseManager;
        }

        [HttpPost("generate/{userId}")]
        public async Task<IActionResult> StartIkigaiProcessing(Guid userId)
        {
            try
            {
                // เรียก Service ให้เริ่มงาน และรับ processId กลับมา
                var processId = await _ikigaiService.GenerateIkigaiAsync(userId);

                // คืนค่า processId กลับไปให้ Frontend (Next.js) เพื่อเอาไปใช้เปิดท่อ SSE
                return Ok(new
                {
                    message = "Ikigai processing started.",
                    processId = processId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        
        [HttpGet("stream/{processId}")]
        public async Task StreamFromN8n(string processId)
        {
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";

            using var writer = new StreamWriter(Response.Body);
            _sseManager.AddClient(processId, writer);

            try
            {
                await Task.Delay(Timeout.Infinite, HttpContext.RequestAborted);
            }
            catch (TaskCanceledException) { /* Client ตัดสาย */ }
            finally { _sseManager.RemoveClient(processId); }
        }

        [HttpPost("webhook/update")]
        public async Task<IActionResult> ReceiveUpdateFromN8n([FromBody] N8nUpdatePayload payload)
        {
            string statusText = payload.Progress switch
            {
                10 => "กำลังวิเคราะห์สิ่งที่คุณรัก...",
                20 => "กำลังวิเคราะห์ประสบการณ์ และค้นหาอาชีพที่เหมาะสำหรับคุณ...",
                60 => "กำลังวิเคราะห์สิ่งที่คุณทำได้ดี และสิ่งที่โลกต้องการ...",
                70 => "กำลังวิเคราะห์สิ่งที่คุณสร้างรายได้ได้...",
                80 => "กำลังวิเคราะห์ \"อิคิไก\" ของคุณ...",
                100 => "ใกล้เสร็จแล้ว กำลังบันทึกผลลัพธ์...",
                _ => "กำลังประมวลผลข้อมูล..."
            };

            IkigaiResultDto? finalDto = null;

            if (payload.Progress == 100 && payload.Result != null)
            {
                if (Guid.TryParse(payload.ProcessId, out Guid id))
                {
                    // รับ DTO ตัวเต็มจาก Service
                    finalDto = await _ikigaiService.SaveFinalResultAsync(id, payload.Result);
                }
            }

            await _sseManager.SendUpdateAsync(payload.ProcessId, new
            {
                status = statusText,
                progress = payload.Progress,
                result = finalDto
            }, payload.Progress);

            return Ok(new { message = "Update processed successfully" });
        }
    }
}