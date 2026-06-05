using eCommerce.Application.DTOs.Identity;
using eCommerce.Application.Filters;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace eCommerce.Tests.Filters;

public class ValidationFilterTests
{
    [Fact]
    public async Task OnActionExecutionAsync_CallsNext_WhenNoArguments()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var filter = new ValidationFilter(serviceProvider, Mock.Of<ILogger<ValidationFilter>>());
        var context = CreateContext(new Dictionary<string, object?>());

        var wasCalled = false;
        await filter.OnActionExecutionAsync(context, () =>
        {
            wasCalled = true;
            return Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), new Mock<Controller>().Object));
        });

        Assert.True(wasCalled);
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task OnActionExecutionAsync_SetsBadRequest_WhenValidationFails()
    {
        var services = new ServiceCollection();
        services.AddScoped<IValidator<LoginUserDTO>, InlineValidator<LoginUserDTO>>(_ =>
        {
            var validator = new InlineValidator<LoginUserDTO>();
            validator.RuleFor(x => x.Email).Must(_ => false).WithMessage("Email invalid");
            return validator;
        });

        var filter = new ValidationFilter(services.BuildServiceProvider(), Mock.Of<ILogger<ValidationFilter>>());
        var context = CreateContext(new Dictionary<string, object?>
        {
            ["request"] = new LoginUserDTO { Email = "bad", Password = "x" }
        });

        await filter.OnActionExecutionAsync(context, () =>
            Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), new Mock<Controller>().Object)));

        var result = Assert.IsType<JsonResult>(context.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task OnActionExecutionAsync_CallsNext_WhenValidationPasses()
    {
        var services = new ServiceCollection();
        services.AddScoped<IValidator<LoginUserDTO>, InlineValidator<LoginUserDTO>>(_ =>
        {
            var validator = new InlineValidator<LoginUserDTO>();
            validator.RuleFor(x => x.Email).NotEmpty();
            return validator;
        });

        var filter = new ValidationFilter(services.BuildServiceProvider(), Mock.Of<ILogger<ValidationFilter>>());
        var context = CreateContext(new Dictionary<string, object?>
        {
            ["request"] = new LoginUserDTO { Email = "ok@test.com", Password = "x" }
        });

        var wasCalled = false;
        await filter.OnActionExecutionAsync(context, () =>
        {
            wasCalled = true;
            return Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), new Mock<Controller>().Object));
        });

        Assert.True(wasCalled);
        Assert.Null(context.Result);
    }

    private static ActionExecutingContext CreateContext(IDictionary<string, object?> arguments)
    {
        var httpContext = new DefaultHttpContext();
        var routeData = new RouteData();
        var actionDescriptor = new ControllerActionDescriptor();
        var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            arguments,
            new Mock<Controller>().Object);
    }
}
