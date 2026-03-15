
using System.Text.Json;
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
        public async Task GetStream(Guid processId)
        {
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";

            var writer = new StreamWriter(Response.Body);
            _sseManager.AddClient(processId, writer);

            try
            {
                // 1. เช็คสถานะปัจจุบันจาก Database ก่อนเป็นอันดับแรก
                var currentStatus = await _ikigaiService.GetStatusOnlyAsync(processId);

                // 2. ดักจับกรณีที่ประมวลผลล้มเหลวไปแล้ว (ป้องกัน User รอเก้อ)
                if (currentStatus == ProcessStatus.Failed)
                {
                    await _sseManager.SendUpdateAsync(processId, new
                    {
                        status = "Error",
                        progress = -1,
                        error = "เกิดข้อผิดพลาดในการประมวลผลก่อนหน้านี้ กรุณาลองใหม่อีกครั้ง"
                    }, -1);
                    return; 
                }

                // 3. กรณีที่ประมวลผลเสร็จสมบูรณ์แล้ว
                if (currentStatus == ProcessStatus.Completed)
                {
                    var existingResult = await _ikigaiService.GetIkigaiResultAsync(processId);
                    if (existingResult != null)
                    {
                        await _sseManager.SendUpdateAsync(processId, new
                        {
                            status = "Completed",
                            progress = 100,
                            result = existingResult
                        }, 100);
                        return; 
                    }
                }

                // 4. ส่งสถานะ "เชื่อมต่อสำเร็จ" กลับไปให้ Frontend อุ่นใจ ว่ายัง Processing อยู่นะ
                if (currentStatus == ProcessStatus.Processing)
                {
                    await _sseManager.SendUpdateAsync(processId, new
                    {
                        status = "Reconnected",
                        progress = 0, 
                        message = "กำลังประมวลผลต่อจากเดิม..."
                    }, 0);
                }

                // 5. เปิด Connection ค้างไว้เพื่อรอรับ Update จาก Webhook ของ n8n
                await Task.Delay(Timeout.Infinite, HttpContext.RequestAborted);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine($"Client disconnected from stream: {processId}");
            }
            finally
            {
                _sseManager.RemoveClient(processId);
                await writer.DisposeAsync();
            }
        }

        [HttpPost("webhook/update")]
        public async Task<IActionResult> ReceiveUpdateFromN8n([FromBody] N8nUpdatePayload payload)
        {
            IkigaiResultDto? finalDto = null;

            if (payload.Progress == 100 && payload.Result != null)
            {

                // รับ DTO ตัวเต็มจาก Service
                finalDto = await _ikigaiService.SaveFinalResultAsync(payload.ProcessId, payload.Result);

            }

            await _sseManager.SendUpdateAsync(payload.ProcessId, new
            {
                status = payload.Progress,
                progress = payload.Progress,
                result = finalDto
            }, payload.Progress);

            return Ok(new { message = "Update processed successfully" });
        }
    }
}