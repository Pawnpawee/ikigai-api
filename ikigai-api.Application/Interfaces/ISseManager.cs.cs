
namespace ikigai_api.Application.Interfaces
{
    public interface ISseManager
    {
        void AddClient(Guid processId, StreamWriter writer);
        void RemoveClient(Guid processId);
        Task SendUpdateAsync(Guid processId, object data, int currentProgress);
    }
}
