namespace CityInfo.API.Models;

public record PaginationMetadata(int TotalItemCount, int PageSize, int CurrentPage)
{
    public int TotalPagesCount => (int) Math.Ceiling(TotalItemCount / (double) PageSize);
}