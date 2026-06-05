using ClientLibrary.Models.Responses;

namespace eCommerce.Frontend.Common.Extensions
{
    public static class ServiceResponseExtensions
    {
        public static string GetErrorMessage<T>(this ServiceResponse<T> response)
        {
            if (response == null)
                return "Unexpected error occurred.";

            if (response.Success)
                return null;

            if (!string.IsNullOrWhiteSpace(response.Message))
                return response.Message;

            return "Something went wrong. Please try again.";
        }

        public static string GetErrorMessage(this LoginResponse response)
        {
            if (response == null)
                return "Unexpected error occurred.";

            if (response.Success)
                return null;

            if (!string.IsNullOrWhiteSpace(response.Message))
                return response.Message;

            return "Something went wrong. Please try again.";
        }
    }
}   
