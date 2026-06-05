using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Application.DTOs.Orders
{
    public class OrderFilterDTO
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public OrderStatus? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
