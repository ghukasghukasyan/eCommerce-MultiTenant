namespace ClientLibrary.Models.Responses
{
    public record ServiceResponse(bool Success=false, string Message = null!);

    public record ServiceResponse<T>(
    bool Success,
    T Data = default,
    string Message = null,
    string ErrorCode = null
);
}

