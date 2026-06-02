using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.ChatData.repositories
{
    public interface IUserRepository
    {
        Task CreateAsync(User user);
        Task DeleteAsync(User user);
        Task<List<User>> GetAllAsync();
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByUsernameAsync(string username);
        Task<List<User>> SearchByUsernameAsync(string query);
        Task UpdateAsync(User user);
        Task UpdatePasswordAsync(Guid id, string passwordHash);
    }
}