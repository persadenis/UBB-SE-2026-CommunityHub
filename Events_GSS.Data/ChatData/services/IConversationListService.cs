using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.ChatData.services
{
    public interface IConversationListService
    {
        Task<List<Conversation>> GetAllAsync(Guid userId);
        Task<Conversation?> GetByIdAsync(Guid conversationId);
        Task<List<Conversation>> GetDmsAsync(Guid userId);
        Task<List<Conversation>> GetFavouritesAsync(Guid userId);
        Task<List<Conversation>> GetGroupsAsync(Guid userId);
        Task<Message?> GetLastMessageAsync(Guid conversationId);
        Task<List<Conversation>> GetUnreadAsync(Guid userId);
        Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId);
        Task<List<Conversation>> SearchAsync(Guid userId, string query);
        Task SetFavouriteAsync(Guid conversationId, Guid userId, bool isFavourite);
    }
}
