namespace eCommerce.Application.DTOs.Responses
{
    public record ServiceResponse(bool Success, string Message = null!, string ErrorCode = null);

    public record ServiceResponse<T>(
    bool Success,
    T Data = default,
    string Message = null,
    string ErrorCode = null
);

}
