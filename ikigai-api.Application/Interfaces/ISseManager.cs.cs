
namespace ikigai_api.Application.Interfaces
{
    public interface ISseManager
    {
        void AddClient(string processId, StreamWriter writer);
        void RemoveClient(string processId);
        Task SendUpdateAsync(string processId, object data, int currentProgress);
    }
}
