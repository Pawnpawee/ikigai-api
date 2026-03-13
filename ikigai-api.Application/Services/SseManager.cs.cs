using System.Collections.Concurrent;
using System.Text.Json;
using ikigai_api.Application.Interfaces;

namespace ikigai_api.Application.Services
{
    public class SseManager : ISseManager
    {
        private readonly ConcurrentDictionary<string, StreamWriter> _clients = new();
        private readonly ConcurrentDictionary<string, int> _lastProgress = new(); // กัน Progress ถอยหลัง

        public void AddClient(string processId, StreamWriter writer)
        {
            _clients.TryAdd(processId, writer);
            _lastProgress.TryAdd(processId, 0);
        }

        public void RemoveClient(string processId)
        {
            _clients.TryRemove(processId, out _);
            _lastProgress.TryRemove(processId, out _);
        }

        public async Task SendUpdateAsync(string processId, object data, int currentProgress)
        {
            // กำหนดให้ -1 คือรหัสพิเศษสำหรับ Error (ไม่ต้องเช็ค Race Condition ปล่อยผ่านทันที)
            if (currentProgress != -1)
            {
                var lastPct = _lastProgress.GetValueOrDefault(processId, 0);

                // ถ้าค่าน้อยกว่าเดิม หรือค่าซ้ำ(ที่ไม่ใช่ 100) ให้เมินทิ้ง
                if (currentProgress < lastPct || (currentProgress == lastPct && currentProgress != 100))
                {
                    return;
                }

                // อัปเดตความจำ
                _lastProgress[processId] = currentProgress;
            }

            if (_clients.TryGetValue(processId, out var writer))
            {
                try
                {
                    var jsonMessage = JsonSerializer.Serialize(data, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    await writer.WriteAsync($"data: {jsonMessage}\n\n");
                    await writer.FlushAsync();
                }
                catch
                {
                    RemoveClient(processId);
                }
            }
        }
    }
}