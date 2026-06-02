// <copyright file="IReputationRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ChatAndEvents.Data.EventsData.Repositories.reputationRepository;

using ChatAndEvents.Data.EventsData.Models;

/// <summary>
/// Defines methods for managing and retrieving user reputation points and tiers in the system.
/// </summary>
public interface IReputationRepository
{
    /// <summary>
    /// Asynchronously retrieves the full reputation score record for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user for whom to retrieve the reputation score.</param>
    /// <returns>The user's reputation score record, or a default score when no row exists yet.</returns>
    Task<UserReputationScore> GetReputationScoreAsync(Guid userId);

    /// <summary>
    /// Asynchronously retrieves the reputation points for a specific user by their user ID.
    /// </summary>
    /// <param name="userId">The ID of the user for whom to retrieve reputation points.</param>
    /// <returns>A task that represents the asynchronous operation, containing the reputation points of the specified user.</returns>
    Task<int> GetReputationPointsAsync(Guid userId);

    /// <summary>
    /// Asynchronously retrieves the tier for a specific user by their user ID.
    /// </summary>
    /// <param name="userId">The ID of the user for whom to retrieve the tier.</param>
    /// <returns>A task that represents the asynchronous operation, containing the tier of the specified user.</returns>
    Task<string> GetTierAsync(Guid userId);

    /// <summary>
    /// Asynchronously sets the reputation points and tier for a specific user by their user ID.
    /// </summary>
    /// <param name="userId">The ID of the user for whom to set the reputation points and tier.</param>
    /// <param name="reputationPoints">The reputation points to set for the user.</param>
    /// <param name="tier">The tier to set for the user.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SetReputationAsync(Guid userId, int reputationPoints, string tier);

    /// <summary>
    /// Asynchronously inserts or updates a user's full reputation score record.
    /// </summary>
    /// <param name="reputationScore">The reputation score to store.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SetReputationAsync(UserReputationScore reputationScore);
}
