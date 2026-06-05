using System.Globalization;
using eCommerce.Application.DTOs.Identity;
using FluentValidation;

namespace eCommerce.Application.Validations.Authentication
{
    public class RegisterUserValidator : AbstractValidator<RegisterUserDTO>
    {
        public RegisterUserValidator()
        {
            RuleFor(user => user.FullName)
                  .NotEmpty().WithMessage(_ => Localize(
                      "Full name is required.",
                      "Անուն-ազգանունը պարտադիր է։",
                      "Полное имя обязательно."));

            RuleFor(user => user.Email)
                .NotEmpty().WithMessage(_ => Localize(
                    "Email is required.",
                    "Էլ. հասցեն պարտադիր է։",
                    "Email обязателен."))
                .EmailAddress().WithMessage(_ => Localize(
                    "A valid email is required.",
                    "Պահանջվում է վավեր էլ. հասցե։",
                    "Требуется корректный email."));

            RuleFor(user => user.Password)
                .NotEmpty().WithMessage(_ => Localize(
                    "Password is required.",
                    "Գաղտնաբառը պարտադիր է։",
                    "Пароль обязателен."))
                .MinimumLength(8).WithMessage(_ => Localize(
                    "Password must be at least 8 characters long.",
                    "Գաղտնաբառը պետք է ունենա առնվազն 8 նիշ։",
                    "Пароль должен содержать минимум 8 символов."))
                .Matches("[A-Z]").WithMessage(_ => Localize(
                    "Password must contain at least one uppercase letter.",
                    "Գաղտնաբառը պետք է պարունակի առնվազն մեկ մեծատառ (A-Z)։",
                    "Пароль должен содержать хотя бы одну заглавную букву (A-Z)."))
                .Matches("[a-z]").WithMessage(_ => Localize(
                    "Password must contain at least one lowercase letter.",
                    "Գաղտնաբառը պետք է պարունակի առնվազն մեկ փոքրատառ։",
                    "Пароль должен содержать хотя бы одну строчную букву."))
                .Matches(@"\d").WithMessage(_ => Localize(
                    "Password must contain at least one digit.",
                    "Գաղտնաբառը պետք է պարունակի առնվազն մեկ թիվ։",
                    "Пароль должен содержать хотя бы одну цифру."))
                .Matches(@"[^\w]").WithMessage(_ => Localize(
                    "Password must contain at least one special character.",
                    "Գաղտնաբառը պետք է պարունակի առնվազն մեկ հատուկ նշան։",
                    "Пароль должен содержать хотя бы один специальный символ."));

        }

        private static string Localize(string english, string armenian, string russian)
        {
            var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            return language switch
            {
                "hy" => armenian,
                "ru" => russian,
                _ => english,
            };
        }
    }
}
