using AutoMapper;
using eCommerce.Application.Constants;
using eCommerce.Application.DTOs.Identity;
using eCommerce.Application.DTOs.Responses;
using eCommerce.Application.Services.Interfaces;
using eCommerce.Application.Services.Interfaces.Authentication;
using eCommerce.Application.Services.Interfaces.Logging;
using eCommerce.Domain.Entities.Identity;
using eCommerce.Domain.Entities.Influencers;
using eCommerce.Domain.Interfaces.Authentication;
using eCommerce.Domain.Interfaces.Influencers;
using Microsoft.Extensions.Configuration;
using System.Web;
using static eCommerce.Domain.Enums.Statuses;

namespace eCommerce.Application.Services.Implementations.Authentication
{
    public class AuthenticationService(ITokenManagement tokenManagement, IUserManagement userManagement, IRoleManagement roleManagement, IAppLogger<AuthenticationService> logger, IMapper mapper, IInfluencerRepository influencerRepository, IEmailService emailService, IConfiguration config) : IAuthenticationService
    {
        public async Task<ServiceResponse> RegisterAsync(RegisterUserDTO user)
        {
            var mappedModel = mapper.Map<AppUser>(user);

            mappedModel.UserName = user.Email;

            var result = await userManagement.RegisterAsync(mappedModel, user.Password);

            if (!result.Succeeded)
            {
                if (result.Errors.FirstOrDefault()?.Code == "UnconfirmedUserExists")
                {
                    var unconfirmedUser = await userManagement.GetByEmailAsync(user.Email);
                    _ = SendConfirmationEmailAsync(unconfirmedUser!);
                    return new ServiceResponse(true, "Account Created");
                }

                return new ServiceResponse(false, result.Errors.FirstOrDefault()?.Description);
            }

            var existingUser = await userManagement.GetByEmailAsync(user.Email);
            var otherUsersExist = await userManagement.AnyOtherUserExistsAsync(existingUser!.Id);
            bool assignedResult = await roleManagement.AddUserToRole(existingUser!, otherUsersExist ? "User" : "Admin");

            if (!assignedResult)
            {
                int removeUserResult = await userManagement.RemoveByEmailAsync(existingUser!.Email!);

                if (removeUserResult <= 0)
                {
                    logger.LogError(
                        new Exception($"User {existingUser.Email} could not be removed after role assignment failure — account is in inconsistent state"),
                        "Role assignment rollback failed");
                }

                return new ServiceResponse(false, "Error occurred while creating account");
            }

            _ = SendConfirmationEmailAsync(existingUser!);

            return new ServiceResponse(true, "Account Created");
        }
        public async Task<ServiceResponse> RegisterInfluencerAsync(RegisterInfluencerDTO influencer)
        {
            if (await userManagement.GetByEmailAsync(influencer.Email) != null)
                return new ServiceResponse(false, "User already exists.");

            var user = new AppUser
            {
                UserName = influencer.Email,
                Email = influencer.Email,
                FullName = influencer.FullName
            };

            var result = await userManagement.RegisterAsync(user, influencer.Password);

            if (!result.Succeeded)
                return new ServiceResponse(false, result.Errors.FirstOrDefault()?.Description);

            await roleManagement.AddUserToRole(user, "Influencer");
            await roleManagement.AddUserToRole(user, "User");

            var influencerProfile = new Influencer
            {
                Id = Guid.NewGuid(),
                FullName = influencer.FullName,
                Email = influencer.Email,
                PhoneNumber = influencer.PhoneNumber,
                UserId = user.Id,
                DefaultCommissionRate = InfluencerConstants.DefaultCommissionRate,
                InstagramAccountUrl = influencer.InstagramAccountUrl,
                TikTokAccountUrl = influencer.TikTokAccountUrl,
                Status = InfluencerStatus.Pending,
                RegisteredAt = DateTime.UtcNow
            };

            await influencerRepository.AddAsync(influencerProfile);

            var createdUser = await userManagement.GetByEmailAsync(influencer.Email);
            _ = SendConfirmationEmailAsync(createdUser!);

            return new ServiceResponse(true, "Influencer created successfully");
        }        
        public async Task<LoginResponse> LoginUserAsync(LoginUserDTO login)
        {
            bool loginResult = await userManagement.LoginAsync(login.Email, login.Password);
            if (!loginResult)
                return new LoginResponse(Message: "Invalid credentials");

            var _user = await userManagement.GetByEmailAsync(login.Email);
            var claims = await userManagement.GetClaimsAsync(_user!.Email!);

            var jwtToken = tokenManagement.GenerateToken(claims);
            string refreshToken = tokenManagement.GetRefreshToken();
            int saveTokenResult;
            bool userHasToken = await tokenManagement.HasTokenForUserAsync(_user.Id);
            if (userHasToken)
                saveTokenResult = await tokenManagement.UpdateRefreshToken(_user.Id, refreshToken);
            else
                saveTokenResult = await tokenManagement.AddRefreshToken(_user.Id, refreshToken);

            return saveTokenResult <= 0 ? new LoginResponse(Message: "Internal error occurred while authenticating") :
                new LoginResponse(Success: true, Token: jwtToken, RefreshToken: refreshToken);

        }
        public async Task<LoginResponse> ReviveTokenAsync(string refreshToken)
        {
            bool validaTokenResult = await tokenManagement.ValidateRefreshToken(refreshToken);
            if (!validaTokenResult)
                return new LoginResponse(Message: "Invalid token");

            string userId = await tokenManagement.GetUserIdByRefreshToken(refreshToken);
            if (string.IsNullOrEmpty(userId))
                return new LoginResponse(Message: "Invalid token");

            AppUser user = await userManagement.GetByIdAsync(userId);
            if (user == null)
                return new LoginResponse(Message: "User not found");

            var claims = await userManagement.GetClaimsAsync(user.Email!);
            string newJwtToken = tokenManagement.GenerateToken(claims);
            string newRefreshToken = tokenManagement.GetRefreshToken();
            await tokenManagement.UpdateRefreshToken(userId, newRefreshToken);
            return new LoginResponse(Success: true, Token: newJwtToken, RefreshToken: newRefreshToken);
        }

        public async Task<ServiceResponse> ConfirmEmailAsync(string userId, string token)
        {
            var user = await userManagement.GetByIdAsync(userId);
            if (user is null)
                return new ServiceResponse(false, "User not found");

            var result = await userManagement.ConfirmEmailAsync(user, token);
            return result.Succeeded
                ? new ServiceResponse(true, "Email confirmed successfully")
                : new ServiceResponse(false, result.Errors.FirstOrDefault()?.Description ?? "Email confirmation failed");
        }

        public async Task<ServiceResponse> ForgotPasswordAsync(string email)
        {
            var user = await userManagement.GetByEmailAsync(email);
            if (user is null)
                return new ServiceResponse(true, "If that email exists, a reset link has been sent");

            var token = await userManagement.GeneratePasswordResetTokenAsync(user);
            var frontendBase = config["Frontend:BaseUrl"]?.TrimEnd('/') ?? "http://localhost";
            var link = $"{frontendBase}/authentication/reset-password?userId={HttpUtility.UrlEncode(user.Id)}&token={HttpUtility.UrlEncode(token)}";

            await emailService.SendPasswordResetAsync(user.Email!, user.FullName, link);

            return new ServiceResponse(true, "If that email exists, a reset link has been sent");
        }

        public async Task<ServiceResponse> ResetPasswordAsync(ResetPasswordDTO dto)
        {
            var user = await userManagement.GetByIdAsync(dto.UserId);
            if (user is null)
                return new ServiceResponse(false, "Invalid request");

            var result = await userManagement.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
                return new ServiceResponse(false, result.Errors.FirstOrDefault()?.Description ?? "Password reset failed");

            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
                await userManagement.UpdateAsync(user);
            }

            return new ServiceResponse(true, "Password reset successfully");
        }

        public async Task LogoutAsync(string userId)
            => await tokenManagement.RevokeRefreshTokenAsync(userId);

        private async Task SendConfirmationEmailAsync(AppUser user)
        {
            try
            {
                var token = await userManagement.GenerateEmailConfirmationTokenAsync(user);
                var frontendBase = config["Frontend:BaseUrl"]?.TrimEnd('/') ?? "http://localhost";
                var link = $"{frontendBase}/authentication/confirm-email?userId={HttpUtility.UrlEncode(user.Id)}&token={HttpUtility.UrlEncode(token)}";
                await emailService.SendEmailConfirmationAsync(user.Email!, user.FullName, link);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send confirmation email");
            }
        }
    }
}
