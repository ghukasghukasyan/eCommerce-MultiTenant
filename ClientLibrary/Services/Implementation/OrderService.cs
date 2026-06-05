using ClientLibrary.Helpers.Interface;
using ClientLibrary.Models.API_s;
using ClientLibrary.Models.Bases;
using ClientLibrary.Models.Orders;
using ClientLibrary.Models.Responses;
using ClientLibrary.Services.Interface;
using static ClientLibrary.Constants.AppConstants;

namespace ClientLibrary.Services.Implementation
{
    public class OrderService(IHttpClientHelper httpClient, IApiCallHelper apiHelper) : IOrderService
    {
        public async Task<ServiceResponse<Guid>> SaveOrder(CreateOrderDTO order)
        {
            var privateClient = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Order.Create,
                Type = ApiCallType.Post,
                Client = privateClient,
                Id = null!,
                Model = order,
            };
            var result = await apiHelper.ApiCallTypeCall<CreateOrderDTO>(apiCall);
            if (result == null)
                return apiHelper.ConnectionError();
            else
                return await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }
        public async Task<ServiceResponse<Guid>> Checkout(CheckoutDTO checkout)
        {
            var privateClient = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Order.Checkout,
                Type = ApiCallType.Post,
                Client = privateClient,
                Id = null!,
                Model = checkout,
            };
            var result = await apiHelper.ApiCallTypeCall<CheckoutDTO>(apiCall);
            if (result == null)
                return apiHelper.ConnectionError();
            else
                return await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }
        public async Task<ServiceResponse<Guid>> UpdateStatus(UpdateOrderStatusDTO updateOrderStatus)
        {
            var privateClient = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Order.UpdateStatus,
                Type = ApiCallType.Update,
                Client = privateClient,
                Id = null!,
                Model = updateOrderStatus,
            };
            var result = await apiHelper.ApiCallTypeCall<UpdateOrderStatusDTO>(apiCall);
            if (result == null)
                return apiHelper.ConnectionError();
            else
                return await apiHelper.GetServiceResponse<ServiceResponse<Guid>>(result);
        }
        public async Task<PagedResult<OrderDTO>> GetAll(OrderFilterDTO filter)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = $"{Order.GetAll}?{BuildQueryString(filter)}",
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
                Id = null!,
            };
            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            return result.IsSuccessStatusCode
                ? await apiHelper.GetServiceResponse<PagedResult<OrderDTO>>(result)
                : new PagedResult<OrderDTO> { Items = [], TotalCount = 0 };
        }

        public async Task<OrderDetailDTO> GetById(Guid id)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = Order.Get,
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
            };
            apiCall.ToString(id);
            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            return result.IsSuccessStatusCode
                ? await apiHelper.GetServiceResponse<OrderDetailDTO>(result)
                : null!;
        }

        public async Task<PagedResult<OrderDTO>> GetUserOrders(OrderFilterDTO filter)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = $"{User.GetAllOrders}?{BuildQueryString(filter)}",
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
                Id = null!,
            };
            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            return result.IsSuccessStatusCode
                ? await apiHelper.GetServiceResponse<PagedResult<OrderDTO>>(result)
                : new PagedResult<OrderDTO> { Items = [], TotalCount = 0 };
        }

        private static string BuildQueryString(OrderFilterDTO filter)
        {
            var parts = new List<string>();
            if (filter.From.HasValue)
                parts.Add($"from={Uri.EscapeDataString(filter.From.Value.ToString("O"))}");
            if (filter.To.HasValue)
                parts.Add($"to={Uri.EscapeDataString(filter.To.Value.ToString("O"))}");
            if (filter.Status.HasValue)
                parts.Add($"status={(int)filter.Status.Value}");
            parts.Add($"page={filter.Page}");
            parts.Add($"pageSize={filter.PageSize}");
            return string.Join("&", parts);
        }

        public async Task<OrderDetailDTO> GeUserOrdertById(Guid id)
        {
            var client = await httpClient.GetPrivateClientAsync();
            var apiCall = new ApiCall
            {
                Route = User.GetOrder,
                Type = ApiCallType.Get,
                Client = client,
                Model = null!,
            };
            apiCall.ToString(id);
            var result = await apiHelper.ApiCallTypeCall<Dummy>(apiCall);
            if (result.IsSuccessStatusCode)
                return await apiHelper.GetServiceResponse<OrderDetailDTO>(result);
            else
                return null!;
        }
    }
}

