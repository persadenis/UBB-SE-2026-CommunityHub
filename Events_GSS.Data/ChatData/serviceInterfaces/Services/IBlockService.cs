using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.ChatData.serviceInterfaces.Services
{
    public interface IBlockService 
    {
        Task BlockUserAsync(Guid blockerUserId, Guid targetUserId);
        Task UnblockUserAsync(Guid blockerUserId, Guid targetUserId);
        Task<List<User>> GetBlockedUsersAsync(Guid targetUserId);
        Task<bool> IsBlockedAsync(Guid blockerId, Guid targetId);
        Task<bool> CheckIfBlockedAsync(Guid blockerUserId, Guid targetUserId);
    }
}
