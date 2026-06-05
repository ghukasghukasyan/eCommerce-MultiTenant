using eCommerce.Application.DTOs.Products.Variants;
using eCommerce.Application.DTOs.Responses;
using eCommerce.Application.Services.Interfaces.Products;
using eCommerce.Domain.Entities.Products;
using eCommerce.Domain.Interfaces.Products;

namespace eCommerce.Application.Services.Implementations.Products
{
    public class VariantService(
     IProductRepository productRepository,
     IVariantRepository variantRepository) : IVariantService
    {
        public async Task<ServiceResponse> GenerateAsync(GenerateVariantsDTO variantDTO)
        {
            var product = await productRepository.GetByIdAsync(variantDTO.ProductId);
            if (product == null)
                return new ServiceResponse(false, "Product not found");

            if (variantDTO.Attributes.Count == 0)
                return new ServiceResponse(false, "No attributes provided");

            // Snapshot existing variants BEFORE deletion so we can carry over stock and price.
            var existing = await variantRepository.GetByProductIdAsync(product.Id);

            await variantRepository.DeleteByProductIdAsync(product.Id);

            var combinations = BuildCombinations(variantDTO.Attributes);

            var variants = combinations.Select(combination =>
            {
                var variant = new ProductVariant
                {
                    ProductId     = product.Id,
                    Price         = InheritPrice(combination, existing) ?? product.BasePrice ?? 0,
                    StockQuantity = InheritStock(combination, existing, combinations),
                    Sku           = GenerateSku(product.Id, product.Name, combination),
                    IsActive      = true
                };

                variant.AttributeValues = combination.Select(c =>
                    new VariantAttributeValue
                    {
                        ProductVariant     = variant,
                        VariantAttributeId = c.Key,
                        Value              = c.Value
                    }).ToList();

                return variant;
            }).ToList();

            await variantRepository.AddRangeAsync(variants);
            await productRepository.SaveAsync();

            return new ServiceResponse(true, "Variants generated successfully");
        }

        public async Task<List<VariantDTO>> GetByProductIdAsync(Guid productId)
        {
            var variants = await variantRepository.GetByProductIdAsync(productId);

            return variants.Select(v => new VariantDTO
            {
                VariantId = v.Id,
                Price     = v.Price,
                Stock     = v.StockQuantity,
                IsActive  = v.IsActive,
                Attributes = v.AttributeValues.ToDictionary(
                    a => a.VariantAttribute.Name,
                    a => a.Value)
            }).ToList();
        }

        public async Task<ServiceResponse> UpdateVariantAsync(UpdateVariantDTO dto)
        {
            var variant = await variantRepository.GetByIdAsync(dto.VariantId);
            if (variant == null)
                return new ServiceResponse(false, "Variant not found");

            variant.Price         = dto.Price;
            variant.StockQuantity = dto.Stock;
            variant.IsActive      = dto.IsActive;

            await productRepository.SaveAsync();
            return new ServiceResponse(true, "Variant updated");
        }

        // ── Stock inheritance ─────────────────────────────────────────────────
        //
        // Three cases based on the relationship between an existing variant's
        // attribute set and the new combination:
        //
        //  PARENT  (existing attrs ⊂ new combo)  →  adding a dimension
        //          e.g. S(10) → S×Red, S×Blue     →  split: 10/2 = 5 each
        //
        //  CHILDREN (existing attrs ⊃ new combo)  →  removing a dimension
        //          e.g. S×Red(5)+S×Blue(5) → S    →  sum: 5+5 = 10
        //
        //  EXACT   (existing attrs == new combo)   →  same dimensions, copy
        //          e.g. S×Red(5) → S×Red           →  5
        //
        //  NONE                                    →  brand-new value, start at 0

        private static int InheritStock(
            Dictionary<Guid, string> newCombo,
            List<ProductVariant> existing,
            List<Dictionary<Guid, string>> allNewCombos)
        {
            // Parent match — existing is a subset of newCombo (dimension was added).
            foreach (var ev in existing)
            {
                var evAttrs = ToDict(ev);
                if (evAttrs.Count == 0) continue;

                if (!IsSubsetOf(evAttrs, newCombo)) continue;

                // Count how many new combinations share this same parent.
                int siblings = allNewCombos.Count(nc => IsSubsetOf(evAttrs, nc));
                return siblings > 0 ? ev.StockQuantity / siblings : 0;
            }

            // Child match — existing is a superset of newCombo (dimension was removed).
            int childSum = existing
                .Where(ev => IsSubsetOf(newCombo, ToDict(ev)))
                .Sum(ev => ev.StockQuantity);

            return childSum;
        }

        // Carry the custom price forward when the combination is an exact or parent match.
        // Returns null when there is no meaningful existing price to inherit.
        private static decimal? InheritPrice(
            Dictionary<Guid, string> newCombo,
            List<ProductVariant> existing)
        {
            // Exact or parent match → inherit the price (price is per-unit, doesn't split).
            var match = existing.FirstOrDefault(ev =>
            {
                var evAttrs = ToDict(ev);
                return evAttrs.Count > 0 && IsSubsetOf(evAttrs, newCombo);
            });

            return match?.Price;
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static Dictionary<Guid, string> ToDict(ProductVariant ev) =>
            ev.AttributeValues.ToDictionary(av => av.VariantAttributeId, av => av.Value);

        // Returns true when every entry in `subset` also exists in `superset` with the same value.
        private static bool IsSubsetOf(
            Dictionary<Guid, string> subset,
            Dictionary<Guid, string> superset) =>
            subset.All(kv =>
                superset.TryGetValue(kv.Key, out var val) &&
                string.Equals(val, kv.Value, StringComparison.OrdinalIgnoreCase));

        private static List<Dictionary<Guid, string>> BuildCombinations(List<VariantAttributeInputDTO> attributes)
        {
            var result = new List<Dictionary<Guid, string>> { new() };

            foreach (var attr in attributes)
            {
                result = [.. result
                    .SelectMany(
                        existing => attr.Values,
                        (existing, value) =>
                        {
                            var dict = new Dictionary<Guid, string>(existing)
                            {
                                [attr.AttributeId] = value
                            };
                            return dict;
                        })];
            }

            return result;
        }

        private static string GenerateSku(
            Guid productId,
            string productName,
            Dictionary<Guid, string> attributes)
        {
            var prefix = productName[..Math.Min(3, productName.Length)].ToUpper();
            var idPart = productId.ToString("N")[..6].ToUpper();

            return prefix + "-" + idPart + "-" +
                string.Join("-", attributes.Values.Select(v => v.ToUpper()));
        }
    }
}
