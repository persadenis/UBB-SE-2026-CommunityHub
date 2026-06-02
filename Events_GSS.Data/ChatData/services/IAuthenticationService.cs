using System;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;

namespace ChatAndEvents.Data.ChatData.services
{
    public interface IAuthenticationService
    {
        Task ChangePasswordAsync(string email, string newPassword);
        Task<User?> LoginAsync(string username, string password);
        Task<User> RegisterAsync(string username, string email, string password, string phone, DateTime? birthday, string? avatarUrl);
    }
}