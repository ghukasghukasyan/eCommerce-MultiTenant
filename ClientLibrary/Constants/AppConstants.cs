using ClientLibrary.Helpers.Interface;

namespace ClientLibrary.Constants
{
    public static class AppConstants
    {
        public static class Product
        {
            public const string Create = "Product";
            public const string Update = "Product";
            public const string Delete = "Product";       
            public const string GetAll = "Product/all";
            public const string GetRecent = "Product/recent";
            public const string Get = "Product/byId";
            public const string GetProductsByCategory = "Product/categoryId";
            public const string Search = "Product/search";
            public const string UpdateImage = "Product";
            public const string Upload = "Product/Upload";
            public const string DeleteImage = "Product/image";
            public const string UpdateImagePosition = "Product/image/position";
        }
        public static class Category
        {
            public const string Create = "Category";
            public const string Update = "Category";
            public const string Delete = "Category";
            public const string GetAll = "Category/all";
            public const string Get = "Category/byId"; 
        }
        public static class Order
        {
            public const string Create = "Order";
            public const string Checkout = "Order/checkout";
            public const string GetAll = "Order/all";
            public const string Get = "Order/byId";
            public const string UpdateStatus = "Order/status";
            public const string CartName = "myCart";
            public const int CartExpirationTime = 7; // 7 days
        }

        public static class User
        {
            public const string GetAllOrders = "User/orders";
            public const string GetOrder = "User/order";
            public const string Profile = "User/profile";
            public const string Address = "User/address";
        }
        public static class Influencer
        {
            public const string Create = "Influencer";
            public const string Update = "Influencer";
            public const string UpdateStatus = "Influencer/status";
            public const string GetAll = "Influencer/all";
            public const string Get = "Influencer/byId";
            public const string Me = "Influencer/me";
            public const string Stats = "Influencer/stats";
            public const string StatsByIdBase = "Influencer";
        }
        public static class Coupon
        {
            public const string Create = "Coupon";
            public const string Update = "Coupon";
            public const string UpdateStatus = "Coupon/status";
            public const string GetAll = "Coupon/all";
            public const string GetByInfluencer = "Coupon/by-influencer";
            public const string Validate = "Coupon/validate";
        }
        public static class Variant
        {
            public const string Generate = "variant/generate";
            public const string GetByProduct = "variant/byId";
            public const string Update = "variant/update";
        }

        public static class VariantAttribute
        {
            public const string Create = "variantattributes";
            public const string GetAll = "variantattributes";
        }

        public static class Dashboard
        {
            public const string GetStats = "Dashboard/stats";
        }
        public static class Payment
        {
            public const string GetAll = "Payment/methods";
        }
        public static class Authentication
        {
            public const string Type = "Bearer";
            public const string Register = "Authentication/register";
            public const string RegisterInfluencer = "Authentication/register/influencer";
            public const string Login = "Authentication/login";
            public const string ReviveToken = "Authentication/refreshToken";
            public const string ConfirmEmail = "Authentication/confirm-email";
            public const string ForgotPassword = "Authentication/forgot-password";
            public const string ResetPassword = "Authentication/reset-password";
            public const string Logout = "Authentication/logout";
        }
        public static class Cookie
        {
            public const string Name = "token";
            public const string RefreshTokenName = "refresh_token";
            public const string Path = "/";
            public const int CookieExpirationTime = 7200;
        }
        public static class ApiCallType
        {
            public const string Get = "get";
            public const string Post = "post";
            public const string Delete = "delete";
            public const string Update = "update";
        }
        public static class ApiClient
        {
            public const string Name = "Blazor-Client";
        }    
        public static class Administration
        {
            public const string AdminRole = "Admin";
        }
    }
}
