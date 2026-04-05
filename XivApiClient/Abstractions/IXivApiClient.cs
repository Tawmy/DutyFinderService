using System.Text.Json;
using Refit;
using XivApiClient.Models;

namespace XivApiClient.Abstractions;

public interface IXivApiClient
{
    [Get("/asset")]
    Task<IApiResponse<Stream>> GetAssetAsync(
        string path,
        string format,
        string? version = null,
        CancellationToken cancellationToken = default);

    [Get("/asset/map/{territory}/{index}")]
    Task<IApiResponse<Stream>> GetMapAsync(
        string territory,
        string index,
        string? version = null,
        CancellationToken cancellationToken = default);

    [Get("/search")]
    Task<SearchResponse<TFields>> SearchAsync<TFields>(
        string? query = null,
        string? sheets = null,
        string? cursor = null,
        uint? limit = null,
        string? language = null,
        string? schema = null,
        string? fields = null,
        string? transient = null,
        string? version = null,
        CancellationToken cancellationToken = default);

    [Get("/search")]
    Task<SearchResponse<JsonElement>> SearchAsync(
        string? query = null,
        string? sheets = null,
        string? cursor = null,
        uint? limit = null,
        string? language = null,
        string? schema = null,
        string? fields = null,
        string? transient = null,
        string? version = null,
        CancellationToken cancellationToken = default);

    [Get("/sheet")]
    Task<ListResponse> ListSheetsAsync(string? version = null, CancellationToken cancellationToken = default);

    [Get("/sheet/{sheet}")]
    Task<SheetResponse<TFields>> ListRowsAsync<TFields>(
        string sheet,
        string? rows = null,
        uint? limit = null,
        string? after = null,
        string? language = null,
        string? schema = null,
        string? fields = null,
        string? transient = null,
        string? version = null,
        CancellationToken cancellationToken = default);

    [Get("/sheet/{sheet}")]
    Task<SheetResponse<JsonElement>> ListRowsAsync(
        string sheet,
        string? rows = null,
        uint? limit = null,
        string? after = null,
        string? language = null,
        string? schema = null,
        string? fields = null,
        string? transient = null,
        string? version = null,
        CancellationToken cancellationToken = default);

    [Get("/sheet/{sheet}/{row}")]
    Task<RowResponse<TFields>> GetRowAsync<TFields>(
        string sheet,
        string row,
        string? language = null,
        string? schema = null,
        string? fields = null,
        string? transient = null,
        string? version = null,
        CancellationToken cancellationToken = default);

    [Get("/sheet/{sheet}/{row}")]
    Task<RowResponse<JsonElement>> GetRowAsync(
        string sheet,
        string row,
        string? language = null,
        string? schema = null,
        string? fields = null,
        string? transient = null,
        string? version = null,
        CancellationToken cancellationToken = default);

    [Get("/version")]
    Task<VersionsResponse> ListVersionsAsync(CancellationToken cancellationToken = default);
}