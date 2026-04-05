namespace XivApiClient.Models;

public record ContentFinderConditionSheet
{
    public required decimal Score { get; init; }
    public required string Sheet { get; init; }
    public required int RowId { get; init; }
}