using System.ComponentModel.DataAnnotations;

namespace eCommerce.Frontend.Pages.Authentication
{
    public class RegisterViewModel : IValidatableObject
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

        // Influencer-only
        public bool IsInfluencer { get; set; }
        public string InstagramUrl { get; set; }
        public string TikTokUrl { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsInfluencer)
            {
                if (string.IsNullOrWhiteSpace(InstagramUrl))
                    yield return new ValidationResult(
                        "Instagram URL is required.",
                        [nameof(InstagramUrl)]);

                if (string.IsNullOrWhiteSpace(TikTokUrl))
                    yield return new ValidationResult(
                        "TikTok URL is required.",
                        [nameof(TikTokUrl)]);
            }

            if (string.IsNullOrEmpty(Password))
                yield break;

            if (Password.Length < 8)
                yield return new ValidationResult(
                    "Password must be at least 8 characters long.",
                    [nameof(Password)]);

            if (!Password.Any(char.IsUpper))
                yield return new ValidationResult(
                    "Password must contain at least one uppercase letter.",
                    [nameof(Password)]);

            if (!Password.Any(char.IsLower))
                yield return new ValidationResult(
                    "Password must contain at least one lowercase letter.",
                    [nameof(Password)]);

            if (!Password.Any(char.IsDigit))
                yield return new ValidationResult(
                    "Password must contain at least one digit.",
                    [nameof(Password)]);

            // mirrors backend: Matches(@"[^\w]") — any char that is not a letter, digit, or underscore
            if (Password.All(c => char.IsLetterOrDigit(c) || c == '_'))
                yield return new ValidationResult(
                    "Password must contain at least one special character.",
                    [nameof(Password)]);
        }
    }
}
