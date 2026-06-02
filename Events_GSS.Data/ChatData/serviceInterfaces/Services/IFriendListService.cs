using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.ChatData.serviceInterfaces.Services
{
    public interface IFriendListService
    {
        Task<List<User>> GetFriendsAsync(Guid targetUserId);
        Task RemoveFriendAsync(Guid currentUserId, Guid targetFriendId);
    }
}
