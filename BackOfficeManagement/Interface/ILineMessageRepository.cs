
namespace BackOfficeManagement.Interface
{
    public interface ILineMessageRepository
    {
        public Task<bool> SendLINEMessage();

        public Task<bool> SendMessageToGroup(string groupId, string input);
    }
}
