using Microsoft.AspNetCore.Components;

namespace eCommerce.Frontend.Components.Base
{
    public abstract class PagedComponentBase : ComponentBase
    {
        protected int Page { get; private set; } = 1;
        protected int PageSize { get; set; } = 20;
        protected int TotalCount { get; set; }

        protected int TotalPages =>
            (int)Math.Ceiling((double)TotalCount / PageSize);

        protected bool CanGoPrev => Page > 1;
        protected bool CanGoNext => Page < TotalPages;

        protected abstract Task LoadDataAsync();

        protected async Task GoToPageAsync(int page)
        {
            if (page < 1 || page > TotalPages)
                return;

            Page = page;
            await LoadDataAsync();
        }

        protected async Task NextPageAsync()
        {
            if (CanGoNext)
                await GoToPageAsync(Page + 1);
        }

        protected async Task PrevPageAsync()
        {
            if (CanGoPrev)
                await GoToPageAsync(Page - 1);
        }

        protected async Task ResetPagingAsync()
        {
            Page = 1;
            await LoadDataAsync();
        }
    }
}
