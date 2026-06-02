using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.ChatData.serviceInterfaces.Services
{
    public interface IReadReceiptService
    {
        Task MarkAsReadAsync(Guid conversationId, Guid userId, Guid messageId);
        Task MarkLatestAsReadAsync(Guid conversationId, Guid userId);
        Task<List<Participant>> GetReadReceiptsAsync(Guid conversationId, Guid messageId);
        Task<int> GetReadByCountAsync(Guid conversationId, Guid messageId);
        Task<int> GetReadByOthersCountAsync(Guid conversationId, Guid messageId, Guid currentUserId);
        Task<Guid?> GetLastReadMessageAsync(Guid conversationId, Guid userId);
        Task<List<Participant>> GetParticipantsAsync(Guid conversationId);
        Task<DateTime?> GetLastReadTimestampAsync(Guid conversationId, Guid userId);
        Task<List<string>> GetReaderUsernamesAsync(Guid conversationId, Guid messageId, Guid? excludeUserId = null);
    }
}