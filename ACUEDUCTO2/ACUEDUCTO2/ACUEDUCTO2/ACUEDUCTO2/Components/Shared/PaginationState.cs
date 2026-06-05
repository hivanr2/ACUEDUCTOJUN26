using System;

namespace ACUEDUCTO2.Components.Shared;

/// <summary>
/// Tracks paging information shared between lists and the paginator component.
/// </summary>
public sealed class PaginationState
{
    private int currentPage = 1;
    private int itemsPerPage = 25;
    private int totalItems;

    public int CurrentPage => currentPage;

    public int ItemsPerPage
    {
        get => itemsPerPage;
        set
        {
            itemsPerPage = value > 0 ? value : 1;
            currentPage = NormalizePage(currentPage);
        }
    }

    public int TotalItems => totalItems;

    public int TotalPages => totalItems == 0 ? 1 : (int)Math.Ceiling(totalItems / (double)ItemsPerPage);

    public bool HasPreviousPage => currentPage > 1;

    public bool HasNextPage => currentPage < TotalPages;

    public int FirstItemIndex => totalItems == 0 ? 0 : ((currentPage - 1) * ItemsPerPage) + 1;

    public int LastItemIndex => totalItems == 0 ? 0 : Math.Min(totalItems, FirstItemIndex + ItemsPerPage - 1);

    public int Skip => totalItems == 0 ? 0 : (currentPage - 1) * ItemsPerPage;

    public void Reset()
    {
        currentPage = 1;
    }

    public void MoveNext()
    {
        if (HasNextPage)
        {
            currentPage++;
        }
    }

    public void MovePrevious()
    {
        if (HasPreviousPage)
        {
            currentPage--;
        }
    }

    public void UpdateTotals(int newTotal)
    {
        totalItems = Math.Max(0, newTotal);
        currentPage = NormalizePage(currentPage);
    }

    private int NormalizePage(int desiredPage)
    {
        var maxPage = Math.Max(1, TotalPages);
        return Math.Clamp(desiredPage, 1, maxPage);
    }
}
