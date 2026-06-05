namespace eCommerce.Application.DTOs.Products
{
    public class UpdateImagePositionDTO
    {
        public Guid ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string ObjectPosition { get; set; } = "center";
    }
}
