// <copyright file="IEventRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ChatAndEvents.Data.EventsData.Repositories.eventRepository;

using System;
using ChatAndEvents.Data.EventsData.Models;

/// <summary>
/// Repository interface for managing event data operations.
/// </summary>
public interface IEventRepository
{
    /// <summary>
    /// Retrieves all public and active events.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of public active events.</returns>
    Task<List<Event>> GetAllPublicActiveAsync();

    /// <summary>
    /// Retrieves an event by its unique identifier.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the event if found; otherwise, null.</returns>
    Task<Event?> GetByIdAsync(int eventId);

    /// <summary>
    /// Retrieves all events administered by a specific user.
    /// </summary>
    /// <param name="adminId">The admin user identifier.</param>
    /// <returns>A list of events where the user is the admin.</returns>
    Task<List<Event>> GetByAdminIdAsync(Guid adminId);

    /// <summary>
    /// Adds a new event to the repository.
    /// </summary>
    /// <param name="eventEntity">The event entity to add.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the identifier of the newly added event.</returns>
    Task<int> AddAsync(Event eventEntity);

    /// <summary>
    /// Updates an existing event in the repository.
    /// </summary>
    /// <param name="eventEntity">The event entity with updated information.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAsync(Event eventEntity);

    /// <summary>
    /// Deletes an event from the repository.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteAsync(int eventId);
    Task IncrementEnrolledCountAsync(int eventId);
    Task DecrementEnrolledCountAsync(int eventId);
}