using System;

namespace LineItem.Models;

/// <summary>
///     A user of the application.
/// </summary>
public class UserModel
{
    /// <summary>
    ///     Database Id.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     Id of the user from the identity provider (Auth0).
    /// </summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>
    ///     Display name of the user.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    ///     Timestamp when database record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Timestamp when database record was last modified.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}