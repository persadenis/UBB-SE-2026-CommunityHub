using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.ChatData.serviceInterfaces.Services
{
    public interface IFriendRequestService
    {
        Task SendFriendRequestAsync(Guid senderUserId, Guid receiverUserId);
        Task<bool> SendFriendRequestByUsernameAsync(Guid senderUserId, string receiverUsername);
        Task AcceptFriendRequestAsync(Guid currentUserId, Guid requesterUserId);
        Task DeclineFriendRequestAsync(Guid currentUserId, Guid requesterUserId);
        Task<List<User>> GetIncomingRequestsAsync(Guid currentUserId);
        Task<FriendStatus?> GetRelationshipStatusAsync(Guid firstUserId, Guid secondUserId);
    }
}
