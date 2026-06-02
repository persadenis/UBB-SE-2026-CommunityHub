// <copyright file="NotificationRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ChatAndEvents.Data.EventsData.Repositories.notificationRepository;

using ChatAndEvents.Data.Database;
using ChatAndEvents.Data.EventsData.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Implements the <see cref="INotificationRepository"/> interface, providing methods to manage and retrieve notifications for users in the system. This class interacts with a SQL database to perform CRUD operations on notifications, allowing for adding new notifications, retrieving notifications by user ID, and deleting notifications by their unique identifier.
/// </summary>
public class NotificationRepository : INotificationRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationRepository"/> class with the specified SQL connection factory. The connection factory is used to create database connections for executing SQL commands related to notifications.
    /// </summary>
    /// <param name="db">The application database context.</param>
    public NotificationRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    /// <summary>
    /// Asynchronously adds a new notification to the data source. This method takes the user ID, title, description, and creation timestamp as parameters to create a new notification entry in the database. The method executes an SQL INSERT command to add the notification to the Notifications table, allowing for non-blocking execution when adding notifications to the system.
    /// </summary>
    /// <param name="userId">The ID of the user for whom the notification is being created.</param>
    /// <param name="title">The title of the notification.</param>
    /// <param name="description">The description or content of the notification.</param>
    /// <param name="createdAt">The timestamp indicating when the notification was created.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task AddAsync(
        Guid userId,
        string title,
        string description,
        DateTime createdAt,
        string type = "General",
        string sourceFeature = "System",
        string? sourceEntityId = null)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        await EnsureNotificationUserAsync(db, userId);

        db.Notifications.Add(new Notification
        {
            UserId = userId,
            Title = title,
            Description = description,
            CreatedAt = createdAt,
            Type = type,
            SourceFeature = sourceFeature,
            SourceEntityId = sourceEntityId,
        });

        await db.SaveChangesAsync();
    }

    private static async Task EnsureNotificationUserAsync(AppDbContext db, Guid userId)
    {
        var userExists = await db.Set<User>().AnyAsync(user => user.UserId == userId);
        if (userExists)
        {
            return;
        }

        var chatUser = await db.Users.FindAsync(userId);
        db.Set<User>().Add(new User
        {
            UserId = userId,
            Name = chatUser?.Username ?? "User",
        });
    }

    /// <summary>
    /// Asynchronously retrieves a list of notifications for a specific user by their user ID. This method executes an SQL SELECT command to fetch notifications from the database, joining the Notifications table with the Users table to include user information and the users_RP_scores table to include reputation points. The results are ordered by the creation timestamp in descending order, allowing for efficient retrieval of a user's notifications along with relevant user details and reputation points.
    /// </summary>
    /// <param name="userId">The ID of the user for whom to retrieve notifications.</param>
    /// <returns>A task that represents the asynchronous operation, containing a list of notifications for the specified user.</returns>
    public async Task<List<Notification>> GetByUserIdAsync(Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Notifications
            .Include(n => n.User)
            .ThenInclude(u => u.ReputationScore)
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> CountUnreadAsync(Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        return await db.Notifications.CountAsync(notification => notification.UserId == userId && !notification.IsRead);
    }

    public async Task MarkAsReadAsync(int notificationId, Guid userId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var notification = await db.Notifications
            .FirstOrDefaultAsync(item => item.Id == notificationId && item.UserId == userId);

        if (notification == null)
        {
            return;
        }

        notification.IsRead = true;
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Asynchronously deletes a notification by its unique identifier. This method executes an SQL DELETE command to remove the specified notification from the Notifications table in the database, allowing for efficient deletion of notifications based on their ID.
    /// </summary>
    /// <param name="notificationId">The ID of the notification to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeleteAsync(int notificationId)
    {
        using var db = await _contextFactory.CreateDbContextAsync();
        var notification = await db.Notifications.FindAsync(notificationId);
        if (notification == null)
        {
            return;
        }

        db.Notifications.Remove(notification);
        await db.SaveChangesAsync();
    }
}
