using ChatAndEvents.Data.EventsData.Models;

namespace ChatAndEvents.Data.EventsData.Services.attendedEventServices
{
    /// <summary>
    /// Service for managing attended events.
    /// </summary>
    public interface IAttendedEventService
    {
        /// <summary>
        /// Gets all attended events for a user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A list of attended events.</returns>
        Task<List<AttendedEvent>> GetAttendedEventsAsync(Guid userId);

        /// <summary>
        /// Gets events by archive status for a user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="isArchived">If true, returns archived events; otherwise, returns non-archived events.</param>
        /// <returns>A list of attended events matching the archive status.</returns>
        Task<List<AttendedEvent>> GetEventsByArchiveStatusAsync(Guid userId, bool isArchived);

        /// <summary>
        /// Gets a specific attended event.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>The attended event, or null if not found.</returns>
        Task<AttendedEvent?> GetAsync(int eventId, Guid userId);

        /// <summary>
        /// Enrolls a user in an event.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AttendEventAsync(int eventId, Guid userId);

        /// <summary>
        /// Removes a user from an event.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task LeaveEventAsync(int eventId, Guid userId);

        /// <summary>
        /// Sets the archived status of an attended event.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="isArchived">The archived status to set.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SetArchivedAsync(int eventId, Guid userId, bool isArchived);

        /// <summary>
        /// Sets the favourite status of an attended event.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="isFavourite">The favourite status to set.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SetFavouriteAsync(int eventId, Guid userId, bool isFavourite);

        /// <summary>
        /// Gets events that both the user and a friend are enrolled in.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="friendId">The friend's user identifier.</param>
        /// <returns>A list of common attended events.</returns>
        Task<List<AttendedEvent>> GetCommonEventsAsync(Guid userId, Guid friendId);
    }
}