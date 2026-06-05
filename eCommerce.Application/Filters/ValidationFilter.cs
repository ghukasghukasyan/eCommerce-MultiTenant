using eCommerce.Application.DTOs;
using eCommerce.Application.DTOs.Responses;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace eCommerce.Application.Filters
{
    public class ValidationFilter(IServiceProvider serviceProvider, ILogger<ValidationFilter> logger) : IAsyncActionFilter
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<ValidationFilter> _logger = logger;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ActionArguments.Any())
            {
                await next();
                return;
            }

            foreach (var arg in context.ActionArguments)
            {
                var model = arg.Value;
                if (model == null) continue;

                Type validatorType = typeof(IValidator<>).MakeGenericType(model.GetType());
                if (_serviceProvider.GetService(validatorType) is not IValidator validator) continue;

                ValidationContext<object> validationContext = new(model);
                ValidationResult validationResult = await validator.ValidateAsync(validationContext);
                
                if (!validationResult.IsValid)
                {
                    string errorMessage = string.Join("\n", validationResult.Errors
                        .Select(e => e.ErrorMessage)
                        .Where(msg => !string.IsNullOrWhiteSpace(msg)));

                    _logger.LogWarning("Validation failed for {Model}: {Errors}", model.GetType().Name, errorMessage);

                    context.Result = new JsonResult(new ServiceResponse(false, errorMessage))
                    {
                        StatusCode = StatusCodes.Status400BadRequest
                    };

                    return;
                }
            }

            await next();
        }
    }
}
