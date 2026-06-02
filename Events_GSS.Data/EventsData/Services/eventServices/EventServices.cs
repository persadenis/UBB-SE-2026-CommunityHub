namespace ChatAndEvents.Data.EventsData.Services.eventServices;

using System;
using ChatAndEvents.Data.EventsData.Messaging;
using ChatAndEvents.Data.EventsData.Models;
using ChatAndEvents.Data.EventsData.Repositories.eventRepository;
using ChatAndEvents.Data.EventsData.Services.reputationService;
using CommunityToolkit.Mvvm.Messaging;
using ChatAndEvents.Data.EventsData.Messaging;

/// <summary>
/// Provides event management services including CRUD operations and filtering.
/// </summary>
public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;

    private readonly IReputationService _reputationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventService"/> class.
    /// </summary>
    /// <param name="eventRepository">The event repository.</param>
    /// <param name="reputationService">The reputation service.</param>
    public EventService(IEventRepository eventRepository, IReputationService reputationService)
    {
        this._eventRepository = eventRepository;
        this._reputationService = reputationService;
    }

    /// <summary>
    /// Gets all public active events.
    /// </summary>
    /// <returns>A list of public active events.</returns>
    public async Task<List<Event>> GetAllPublicActiveEventsAsync()
        => await this._eventRepository.GetAllPublicActiveAsync();

    /// <summary>
    /// Gets an event by its identifier.
    /// </summary>
    /// <param name="eventId">The event identifier.</param>
    /// <returns>The event if found; otherwise, null.</returns>
    public async Task<Event?> GetEventByIdAsync(int eventId)
        => await this._eventRepository.GetByIdAsync(eventId);

    /// <summary>
    /// Creates a new event.
    /// </summary>
    /// <param name="eventEntity">The event to create.</param>
    /// <returns>The created event identifier.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the user's reputation is too low to create events.</exception>
    public async Task<int> CreateEventAsync(Event eventEntity)
    {
        if (!await this._reputationService.CanCreateEventsAsync(eventEntity.Admin.UserId))
        {
            throw new InvalidOperationException("Your reputation is too low to create events (below -700 RP).");
        }

        int eventId = await this._eventRepository.AddAsync(eventEntity);
        WeakReferenceMessenger.Default.Send(
            new ReputationMessage(eventEntity.Admin.UserId, ReputationAction.EventCreated));
        return eventId;
    }

    /// <summary>
    /// Updates an existing event.
    /// </summary>
    /// <param name="eventEntity">The event to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task UpdateEventAsync(Event eventEntity)
        => await this._eventRepository.UpdateAsync(eventEntity);

    /// <summary>
    /// Deletes an event by its identifier.
    /// </summary>
    /// <param name="eventId">The event identifier.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteEventAsync(int eventId)
    {
        var @event = await this._eventRepository.GetByIdAsync(eventId);
        await this._eventRepository.DeleteAsync(eventId);
        if (@event?.Admin != null)
        {
            WeakReferenceMessenger.Default.Send(
                new ReputationMessage(@event.Admin.UserId, ReputationAction.EventCancelled));
        }
    }

    /// <summary>
    /// Filters events by category.
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    /// <returns>A list of events matching the category.</returns>
    public async Task<List<Event>> FilterByCategoryAsync(string category)
    {
        var events = await this._eventRepository.GetAllPublicActiveAsync();
        return events.Where(@event => @event.Category != null &&
            @event.Category.Title.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Filters events by location.
    /// </summary>
    /// <param name="location">The location to filter by.</param>
    /// <returns>A list of events matching the location.</returns>
    public async Task<List<Event>> FilterByLocationAsync(string location)
    {
        var events = await this._eventRepository.GetAllPublicActiveAsync();
        return events.Where(@event => @event.Name.Contains(location, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Filters events by a specific date.
    /// </summary>
    /// <param name="date">The date to filter by.</param>
    /// <returns>A list of events on the specified date.</returns>
    public async Task<List<Event>> FilterByDateAsync(DateTime date)
    {
        var events = await this._eventRepository.GetAllPublicActiveAsync();
        return events.Where(@event => @event.StartDateTime.Date == date.Date).ToList();
    }

    /// <summary>
    /// Filters events by a date range.
    /// </summary>
    /// <param name="from">The start date of the range.</param>
    /// <param name="to">The end date of the range.</param>
    /// <returns>A list of events within the specified date range.</returns>
    public async Task<List<Event>> FilterByDateRangeAsync(DateTime from, DateTime to)
    {
        var events = await this._eventRepository.GetAllPublicActiveAsync();
        return events.Where(@event => @event.StartDateTime.Date >= from.Date &&
            @event.StartDateTime.Date <= to.Date).ToList();
    }

    /// <summary>
    /// Searches events by title.
    /// </summary>
    /// <param name="title">The title to search for.</param>
    /// <returns>A list of events matching the title.</returns>
    public async Task<List<Event>> SearchByTitleAsync(string title)
    {
        var events = await this._eventRepository.GetAllPublicActiveAsync();
        return events.Where(@event => @event.Name.Contains(title, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Gets all events created by a specific user.
    /// </summary>
    /// <param name="adminId">The admin user identifier.</param>
    /// <returns>A list of events administered by the user.</returns>
    public async Task<List<Event>> GetMyEventsAsync(Guid adminId)
        => await this._eventRepository.GetByAdminIdAsync(adminId);
}
