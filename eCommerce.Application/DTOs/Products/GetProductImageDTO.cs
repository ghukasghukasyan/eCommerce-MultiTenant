namespace eCommerce.Application.DTOs.Products
{
    public class GetProductImageDTO
    {
        public string Base64Data { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }
}
