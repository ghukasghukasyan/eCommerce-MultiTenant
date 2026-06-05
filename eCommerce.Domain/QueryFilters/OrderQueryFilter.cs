using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Domain.QueryFilters
{
    public class OrderQueryFilter
    {
        public DateTime? From { get; init; }
        public DateTime? To { get; init; }
        public OrderStatus? Status { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
    }
}
