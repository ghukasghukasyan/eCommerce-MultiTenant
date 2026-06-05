using eCommerce.Application.DTOs.Orders;
using FluentValidation;

namespace eCommerce.Application.Validations.Orders
{
    public class CreateOrderValidator : AbstractValidator<CreateOrderDTO>
    {
        public CreateOrderValidator()
        {
            RuleFor(o => o.PaymentMethodId)
                .NotEmpty().WithMessage("Payment method is required.");

            RuleFor(o => o.Items)
                .NotEmpty().WithMessage("Order must contain at least one item.");

            RuleForEach(o => o.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.VariantId)
                    .NotEmpty().WithMessage("Variant ID is required.");

                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be at least 1.");
            });

            RuleFor(o => o.ShippingDetail).NotNull().WithMessage("Shipping details are required.");

            When(o => o.ShippingDetail != null, () =>
            {
                RuleFor(o => o.ShippingDetail.FullName)
                    .NotEmpty().WithMessage("Recipient full name is required.");

                RuleFor(o => o.ShippingDetail.PhoneNumber)
                    .NotEmpty().WithMessage("Phone number is required.");

                RuleFor(o => o.ShippingDetail.City)
                    .NotEmpty().WithMessage("City is required.");

                RuleFor(o => o.ShippingDetail.Address)
                    .NotEmpty().WithMessage("Address is required.");
            });
        }
    }
}
