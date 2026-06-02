using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.EventsData.Repositories
{
    public interface IMemoryRepository
    {
        Task<List<Memory>> GetByEventAsync(int eventId);
        Task<int> AddAsync(Memory memory);
        Task DeleteAsync(int memoryId);
        Task AddLikeAsync(int memoryId, Guid userId);
        Task RemoveLikeAsync(int memoryId, Guid userId);
        //maybe
        Task<List<Guid>> GetLikesAsync(int memoryId);
        Task<Memory?> GetByIdAsync(int memoryId);
    }
}