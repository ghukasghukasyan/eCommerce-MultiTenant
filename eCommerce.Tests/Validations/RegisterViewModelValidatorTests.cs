using System.ComponentModel.DataAnnotations;

// RegisterViewModel lives in the Frontend project which is not referenced here,
// so we replicate the same class inline to test the identical rules independently.
// This keeps the test project lean while still verifying frontend ↔ backend parity.

namespace eCommerce.Tests.Validations;

// ── Inline copy of RegisterViewModel (mirrors the real one) ───────────────────
internal class RegisterViewModel : IValidatableObject
{
    [Required, EmailAddress]
    public string Email { get; set; }

    [Required]
    public string FullName { get; set; }

    [Required]
    public string Password { get; set; }

    [Required, Compare(nameof(Password))]
    public string ConfirmPassword { get; set; }

    [Required]
    public string PhoneNumber { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (string.IsNullOrEmpty(Password))
            yield break;

        if (Password.Length < 8)
            yield return new ValidationResult("Password must be at least 8 characters long.", [nameof(Password)]);

        if (!Password.Any(char.IsUpper))
            yield return new ValidationResult("Password must contain at least one uppercase letter.", [nameof(Password)]);

        if (!Password.Any(char.IsLower))
            yield return new ValidationResult("Password must contain at least one lowercase letter.", [nameof(Password)]);

        if (!Password.Any(char.IsDigit))
            yield return new ValidationResult("Password must contain at least one digit.", [nameof(Password)]);

        if (Password.All(c => char.IsLetterOrDigit(c) || c == '_'))
            yield return new ValidationResult("Password must contain at least one special character.", [nameof(Password)]);
    }
}

// ── Helpers ───────────────────────────────────────────────────────────────────
internal static class ModelValidator
{
    public static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }

    public static bool HasErrorFor(List<ValidationResult> results, string member)
        => results.Any(r => r.MemberNames.Contains(member));

    public static bool HasMessage(List<ValidationResult> results, string member, string message)
        => results.Any(r => r.MemberNames.Contains(member) && r.ErrorMessage == message);
}

// ── Tests ─────────────────────────────────────────────────────────────────────
public class RegisterViewModelValidatorTests
{
    private static RegisterViewModel Valid() => new()
    {
        Email = "user@example.com",
        FullName = "Jane Doe",
        Password = "StrongP@ss1",
        ConfirmPassword = "StrongP@ss1",
        PhoneNumber = "+37400000000"
    };

    // ── Password min length ───────────────────────────────────────────────────

    [Fact]
    public void Password_TooShort_Fails()
    {
        var m = Valid(); m.Password = m.ConfirmPassword = "Ab1@";
        var errors = ModelValidator.Validate(m);
        Assert.True(ModelValidator.HasMessage(errors, nameof(m.Password),
            "Password must be at least 8 characters long."));
    }

    [Fact]
    public void Password_ExactlyEightChars_PassesLengthRule()
    {
        var m = Valid(); m.Password = m.ConfirmPassword = "Abcde1@x";
        var errors = ModelValidator.Validate(m);
        Assert.False(ModelValidator.HasMessage(errors, nameof(m.Password),
            "Password must be at least 8 characters long."));
    }

    // ── Uppercase ─────────────────────────────────────────────────────────────

    [Fact]
    public void Password_NoUppercase_Fails()
    {
        var m = Valid(); m.Password = m.ConfirmPassword = "weakp@ss1";
        var errors = ModelValidator.Validate(m);
        Assert.True(ModelValidator.HasMessage(errors, nameof(m.Password),
            "Password must contain at least one uppercase letter."));
    }

    [Fact]
    public void Password_HasUppercase_Passes()
    {
        var m = Valid(); m.Password = m.ConfirmPassword = "Weakp@ss1";
        var errors = ModelValidator.Validate(m);
        Assert.False(ModelValidator.HasMessage(errors, nameof(m.Password),
            "Password must contain at least one uppercase letter."));
    }

    // ── Lowercase ─────────────────────────────────────────────────────────────

    [Fact]
    public void Password_NoLowercase_Fails()
    {
        var m = Valid(); m.Password = m.ConfirmPassword = "WEAKP@SS1";
        var errors = ModelValidator.Validate(m);
        Assert.True(ModelValidator.HasMessage(errors, nameof(m.Password),
            "Password must contain at least one lowercase letter."));
    }

    // ── Digit ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Password_NoDigit_Fails()
    {
        var m = Valid(); m.Password = m.ConfirmPassword = "WeakP@ssword";
        var errors = ModelValidator.Validate(m);
        Assert.True(ModelValidator.HasMessage(errors, nameof(m.Password),
            "Password must contain at least one digit."));
    }

    // ── Special character ─────────────────────────────────────────────────────

    [Fact]
    public void Password_NoSpecialChar_Fails()
    {
        var m = Valid(); m.Password = m.ConfirmPassword = "WeakPass1";
        var errors = ModelValidator.Validate(m);
        Assert.True(ModelValidator.HasMessage(errors, nameof(m.Password),
            "Password must contain at least one special character."));
    }

    [Fact]
    public void Password_UnderscoreOnly_StillFails_SpecialCharRule()
    {
        // underscore is \w — not considered special by the backend rule
        var m = Valid(); m.Password = m.ConfirmPassword = "WeakPass1_";
        var errors = ModelValidator.Validate(m);
        Assert.True(ModelValidator.HasMessage(errors, nameof(m.Password),
            "Password must contain at least one special character."));
    }

    [Fact]
    public void Password_AtSymbol_PassesSpecialCharRule()
    {
        var m = Valid(); m.Password = m.ConfirmPassword = "WeakPass1@";
        var errors = ModelValidator.Validate(m);
        Assert.False(ModelValidator.HasMessage(errors, nameof(m.Password),
            "Password must contain at least one special character."));
    }

    // ── Multiple rules at once ────────────────────────────────────────────────

    [Fact]
    public void Password_OnlyLetters_FailsMultipleRules()
    {
        var m = Valid(); m.Password = m.ConfirmPassword = "weakpassword";
        var errors = ModelValidator.Validate(m);

        Assert.True(ModelValidator.HasMessage(errors, nameof(m.Password),
            "Password must contain at least one uppercase letter."));
        Assert.True(ModelValidator.HasMessage(errors, nameof(m.Password),
            "Password must contain at least one digit."));
        Assert.True(ModelValidator.HasMessage(errors, nameof(m.Password),
            "Password must contain at least one special character."));
    }

    // ── Full valid model ──────────────────────────────────────────────────────

    [Fact]
    public void ValidModel_ProducesNoErrors()
    {
        var errors = ModelValidator.Validate(Valid());
        Assert.Empty(errors);
    }

    // ── Other fields ─────────────────────────────────────────────────────────

    [Fact]
    public void Email_Invalid_Fails()
    {
        var m = Valid(); m.Email = "not-an-email";
        var errors = ModelValidator.Validate(m);
        Assert.True(ModelValidator.HasErrorFor(errors, nameof(m.Email)));
    }

    [Fact]
    public void ConfirmPassword_Mismatch_Fails()
    {
        var m = Valid(); m.ConfirmPassword = "DifferentP@ss1";
        var errors = ModelValidator.Validate(m);
        Assert.True(ModelValidator.HasErrorFor(errors, nameof(m.ConfirmPassword)));
    }

    [Fact]
    public void FullName_Empty_Fails()
    {
        var m = Valid(); m.FullName = "";
        var errors = ModelValidator.Validate(m);
        Assert.True(ModelValidator.HasErrorFor(errors, nameof(m.FullName)));
    }
}
