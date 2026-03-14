
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
            // กำหนด Header ให้เป็นรูปแบบของ Server-Sent Events
            Response.Headers["Content-Type"] = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";

            var writer = new StreamWriter(Response.Body);

            // Add Client เข้าไปใน SseManager ก่อน เพื่อให้พร้อมใช้งาน SendUpdateAsync
            _sseManager.AddClient(processId, writer);

            try
            {
                // 2. ดึงข้อมูลจาก Database ผ่าน Service (เช็คว่ามี Final Data หรือยัง)
                var existingResult = await _ikigaiService.GetIkigaiResultAsync(processId);

                if (existingResult != null)
                {
                    // 3. ถ้า Process เคยทำเสร็จแล้ว (มีข้อมูล)
                    // ให้ใช้ SendUpdateAsync ยิงข้อมูลกลับไปทันที ด้วยโครงสร้างที่ Frontend รอ Map (result = existingResult)
                    await _sseManager.SendUpdateAsync(processId, new
                    {
                        status = "Completed",
                        progress = 100,
                        result = existingResult
                    }, 100);
                }

                // 4. เปิด Connection ค้างไว้เพื่อรอรับ Update จาก Background Task (n8n)
                // หรือรอให้ฝั่ง Frontend สั่ง eventSource.close() เมื่อได้รับ progress = 100
                await Task.Delay(Timeout.Infinite, HttpContext.RequestAborted);
            }
            catch (TaskCanceledException)
            {
                // จะเข้ามาที่นี่เมื่อ Frontend สั่ง close() หรือ User ปิดหน้าเว็บ
                Console.WriteLine($"Client disconnected from stream: {processId}");
            }
            finally
            {
                // 5. ลบ Client ออกเสมอเมื่อจบการทำงาน ป้องกัน Memory Leak
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