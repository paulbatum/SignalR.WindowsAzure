using System.Threading.Tasks;

namespace SignalR.ScaleOut
{
    public interface IMessageIdGenerator
    {
        Task<long> GenerateMessageId(string key);
    }
}