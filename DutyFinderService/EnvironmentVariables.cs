namespace DutyFinderService;

/// <summary>
///     Environment variables, to avoid typos.
/// </summary>
public static class EnvironmentVariables
{
    /// <summary>
    ///     EFCore connection string for Postgres database.
    /// </summary>
    public const string ConnectionString = "CONNECTION_STRING";

    /// <summary>
    ///     Used for JWT Token validation and Swagger UI token retrieval.
    /// </summary>
    public const string AuthAuthority = "AUTH_AUTHORITY";

    /// <summary>
    ///     Used for JWT Token validation and Swagger UI scopes.
    /// </summary>
    public const string AuthAudience = "AUTH_AUDIENCE";

    /// <summary>
    ///     Current FFXIV patch. A new value will force images to be refreshed upon startup.
    /// </summary>
    public const string FfxivPatch = "FFXIV_PATCH";
}