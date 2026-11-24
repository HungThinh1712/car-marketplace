using System.Text.RegularExpressions;

namespace CarMarketplace.Api.Services;

/// <summary>Shared rules for names, emails, passwords, and phone numbers.</summary>
public static partial class ContactRules
{
    /// <summary>Display name and inquiry name field message.</summary>
    public const string NameMessage = "Name must be 1 to 80 characters.";

    /// <summary>Account display name field message.</summary>
    public const string DisplayNameMessage = "Display name must be 1 to 80 characters.";

    /// <summary>Email format field message.</summary>
    public const string EmailMessage = "Enter a valid email address.";

    /// <summary>Password length field message.</summary>
    public const string PasswordMessage = "Password must be at least 8 and at most 128 characters.";

    /// <summary>Duplicate account message.</summary>
    public const string DuplicateEmailMessage = "That email is already in use.";

    /// <summary>Failed sign-in message. It does not say which field failed.</summary>
    public const string SignInMessage = "The email or password is incorrect.";

    /// <summary>Inquiry message length field message.</summary>
    public const string MessageMessage = "Message must be 10 to 1000 characters.";

    /// <summary>Optional phone field message.</summary>
    public const string PhoneMessage = "Phone must contain 7 to 20 digits and may include spaces, hyphens, parentheses, and a leading plus sign.";

    /// <summary>True when the value has a local part, an @ sign, and a domain with a dot.</summary>
    public static bool IsValidEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var email = value.Trim();
        return email.Length <= 254 && EmailPattern().IsMatch(email);
    }

    /// <summary>True when the password is 8 to 128 characters. The password is not trimmed.</summary>
    public static bool IsValidPassword(string? password) =>
        password is not null && password.Length is >= 8 and <= 128;

    /// <summary>True when the trimmed name is 1 to 80 characters.</summary>
    public static bool IsValidName(string? value)
    {
        var name = value?.Trim() ?? "";
        return name.Length is >= 1 and <= 80;
    }

    /// <summary>True when the phone is empty or contains 7 to 20 digits and only the allowed symbols.</summary>
    public static bool IsValidOptionalPhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var phone = value.Trim();
        if (!PhonePattern().IsMatch(phone))
        {
            return false;
        }

        var digits = 0;
        foreach (var character in phone)
        {
            if (char.IsDigit(character))
            {
                digits++;
            }
        }

        return digits is >= 7 and <= 20;
    }

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailPattern();

    [GeneratedRegex(@"^\+?[0-9 ()\-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex PhonePattern();
}
