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
            var key = processId.ToLower().Trim();
            _clients.TryAdd(key, writer);
            _lastProgress.TryAdd(key, 0);
        }

        public void RemoveClient(string processId)
        {
            var key = processId.ToLower().Trim();
            _clients.TryRemove(key, out _);
            _lastProgress.TryRemove(key, out _);
        }

        public async Task SendUpdateAsync(string processId, object data, int currentProgress)
        {
            var key = processId.ToLower().Trim();

            if (_clients.TryGetValue(key, out var writer))
            {
                try
                {
                    var jsonMessage = JsonSerializer.Serialize(data, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    await writer.WriteAsync($"data: {jsonMessage}\n\n");
                    await writer.FlushAsync();
                    Console.WriteLine($"Successfully pushed update for {key}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Push failed for {key}: {ex.Message}");
                    RemoveClient(processId);
                }
            }
            else
            {
                // 🌟 ถ้าเข้าตรงนี้ แปลว่า ID ที่ n8n ส่งมา ไม่ตรงกับที่เปิดท่อไว้
                Console.WriteLine($"No active SSE client for ID: {key}");
            }
        }
    }
}