using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.ChatData.services
{
    public interface ISearchService
    {
        Task<List<Message>> SearchMessagesAsync(Guid conversationId, Guid userId, string query);
        Task<List<User>> SearchUsersAsync(string query);
        Task<List<User>> SearchUsersForAddMemberAsync(Guid conversationId, string query);
    }
}