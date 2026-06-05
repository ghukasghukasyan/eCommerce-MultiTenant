using System.Globalization;
using eCommerce.Application.DTOs.Identity;
using eCommerce.Application.Validations.Authentication;
using FluentValidation.TestHelper;

namespace eCommerce.Tests.Validations;

public class AuthValidatorTests
{
    [Fact]
    public void LoginUserValidator_ShouldRequireEmailAndPassword()
    {
        var validator = new LoginUserValidator();
        var model = new LoginUserDTO { Email = string.Empty, Password = string.Empty };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterUserValidator_ShouldRejectWeakPasswordAndInvalidEmail()
    {
        var validator = new RegisterUserValidator();
        var model = new RegisterUserDTO
        {
            FullName = string.Empty,
            Email = "not-an-email",
            Password = "weak",
            PhoneNumber = "+37455676766",
        };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.FullName);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterUserValidator_ShouldRejectPasswordWithoutUppercase_AZ_InEnglish()
    {
        using var _ = new UiCultureScope("en");

        var validator = new RegisterUserValidator();
        var model = new RegisterUserDTO
        {
            FullName = "Jane Doe",
            Email = "jane@example.com",
            Password = "strongp@ss1",
            PhoneNumber = "+37455676766",
        };

        var result = validator.TestValidate(model);

        result
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one uppercase letter.");
    }

    [Fact]
    public void RegisterUserValidator_ShouldRejectPasswordWithoutUppercase_AZ_InArmenian()
    {
        using var _ = new UiCultureScope("hy");

        var validator = new RegisterUserValidator();
        var model = new RegisterUserDTO
        {
            FullName = "Jane Doe",
            Email = "jane@example.com",
            Password = "strongp@ss1",
            PhoneNumber = "+37455676766",
        };

        var result = validator.TestValidate(model);

        result
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Գաղտնաբառը պետք է պարունակի առնվազն մեկ մեծատառ (A-Z)։");
    }

    [Fact]
    public void RegisterUserValidator_ShouldRejectPasswordWithoutUppercase_AZ_InRussian()
    {
        using var _ = new UiCultureScope("ru");

        var validator = new RegisterUserValidator();
        var model = new RegisterUserDTO
        {
            FullName = "Jane Doe",
            Email = "jane@example.com",
            Password = "strongp@ss1",
            PhoneNumber = "+37455676766",
        };

        var result = validator.TestValidate(model);

        result
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Пароль должен содержать хотя бы одну заглавную букву (A-Z).");
    }

    [Fact]
    public void RegisterUserValidator_ShouldAcceptStrongPasswordWithUppercase_AZ()
    {
        var validator = new RegisterUserValidator();
        var model = new RegisterUserDTO
        {
            FullName = "Jane Doe",
            Email = "jane@example.com",
            Password = "StrongP@ss1",
            PhoneNumber = "+37455676766",
        };

        var result = validator.TestValidate(model);

        result.ShouldNotHaveValidationErrorFor(x => x.FullName);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    private sealed class UiCultureScope : IDisposable
    {
        private readonly CultureInfo _originalUiCulture;

        public UiCultureScope(string cultureName)
        {
            _originalUiCulture = CultureInfo.CurrentUICulture;
            CultureInfo.CurrentUICulture = new CultureInfo(cultureName);
        }

        public void Dispose()
        {
            CultureInfo.CurrentUICulture = _originalUiCulture;
        }
    }
}
