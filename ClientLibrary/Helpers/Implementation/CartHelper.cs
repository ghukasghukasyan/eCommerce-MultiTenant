using ClientLibrary.Constants;
using ClientLibrary.Helpers.Interface;
using ClientLibrary.Models.Orders;
using ClientLibrary.States;
using System.Text.Json;

namespace ClientLibrary.Helpers.Implementation
{
    public class CartHelper(
        ICookieService cookie,
        CartState cartState)
    {
        private readonly ICookieService _cookie = cookie;
        private readonly CartState _cartState = cartState;

        public async Task AddToCart(
            Guid variantId,
            Guid productId,
            string productName,
            string imageUrl,
            decimal price,
            int quantity = 1,
            Dictionary<string, string>? variantAttributes = null)
        {
            var carts = await GetCart();

            var item = carts.FirstOrDefault(x => x.VariantId == variantId);

            if (item == null)
            {
                carts.Add(new CartItemDTO
                {
                    VariantId = variantId,
                    ProductId = productId,
                    ProductName = productName,
                    ImageUrl = imageUrl,
                    UnitPrice = price,
                    Quantity = quantity,
                    VariantAttributes = variantAttributes
                });
            }
            else
            {
                item.Quantity += quantity;
            }

            await SaveCart(carts);
            _cartState.SetCount(carts.Sum(x => x.Quantity));
        }

        public async Task RemoveFromCart(Guid variantId)
        {
            var carts = await GetCart();

            var item = carts.FirstOrDefault(x => x.VariantId == variantId);

            if (item != null)
            {
                carts.Remove(item);
                await SaveCart(carts);
                _cartState.SetCount(carts.Sum(x => x.Quantity));
            }
        }

        public async Task<int> LoadCartCount()
        {
            var carts = await GetCart();

            int count = carts.Sum(x => x.Quantity);

            _cartState.SetCount(count);

            return count;
        }

        public async Task<List<CartItemDTO>> GetCart()
        {
            var json = await _cookie.GetAsync(AppConstants.Order.CartName);

            if (string.IsNullOrEmpty(json))
                return new List<CartItemDTO>();

            try
            {
                return JsonSerializer.Deserialize<List<CartItemDTO>>(json) ?? new List<CartItemDTO>();
            }
            catch (JsonException)
            {
                return new List<CartItemDTO>();
            }
        }

        public async Task SaveCart(List<CartItemDTO> carts)
        {
            await _cookie.SetAsync(
                AppConstants.Order.CartName,
                JsonSerializer.Serialize(carts),
                AppConstants.Order.CartExpirationTime,
                "/");
        }
    }
}