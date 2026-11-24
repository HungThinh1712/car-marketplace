using CarMarketplace.Api.Exceptions;

namespace CarMarketplace.Api.Services;

/// <summary>Collects field messages and throws when any rule failed.</summary>
public sealed class ValidationErrors
{
    private readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);

    /// <summary>Adds one field message.</summary>
    public void Add(string key, string message)
    {
        if (!_errors.TryGetValue(key, out var messages))
        {
            messages = [];
            _errors[key] = messages;
        }

        messages.Add(message);
    }

    /// <summary>True when at least one field failed.</summary>
    public bool HasErrors => _errors.Count > 0;

    /// <summary>Throws when any field failed. The detail is safe to show.</summary>
    public void ThrowIfAny(string detail)
    {
        if (!HasErrors)
        {
            return;
        }

        var errors = _errors.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.ToArray(),
            StringComparer.Ordinal);
        throw new FieldValidationException(detail, errors);
    }
}
