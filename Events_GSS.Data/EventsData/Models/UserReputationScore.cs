// <copyright file="UserReputationScore.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace ChatAndEvents.Data.EventsData.Models
{
    using System;

    /// <summary>
    /// Represents a user's reputation score record stored in the database.
    /// Matches table: users_RP_scores.
    /// </summary>
    public class UserReputationScore
    {
        /// <summary>
        /// Gets or sets the user id. This is the primary key and foreign key to the Users table.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the reputation points.
        /// </summary>
        public int ReputationPoints { get; set; }

        /// <summary>
        /// Gets or sets the tier name.
        /// </summary>
        public string Tier { get; set; } = "Newcomer";

        /// <summary>
        /// Gets or sets the navigation property to the user.
        /// </summary>
        public User? User { get; set; }
    }
}
