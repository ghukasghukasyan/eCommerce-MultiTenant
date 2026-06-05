using ClientLibrary.Models.Orders;

namespace ClientLibrary.Services.Interface
{
    public interface IPaymentMethodService
    {
        Task<IEnumerable<GetPaymentMethodDTO>> GetPaymentMethods();
    }
}
