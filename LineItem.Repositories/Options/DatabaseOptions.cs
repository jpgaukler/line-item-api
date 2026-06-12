// ReSharper disable MemberCanBePrivate.Global

namespace LineItem.Repositories.Options;

/// <summary>
///     Options class for configuring database connection.
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    ///     The host address of the database server.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    ///     The port number of the database server.
    /// </summary>
    public string Port { get; set; } = string.Empty;

    /// <summary>
    ///     The name of the target database.
    /// </summary>
    public string Database { get; set; } = string.Empty;

    /// <summary>
    ///     The username to connect to the database.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    ///     The password to connect to the database.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    public string ConnectionString =>
        $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};";
}