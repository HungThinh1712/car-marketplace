namespace CarMarketplace.Api.Services;

/// <summary>Listing facts before they are normalized.</summary>
/// <param name="Make">Make.</param>
/// <param name="Model">Model.</param>
/// <param name="Year">Model year.</param>
/// <param name="Price">Asking price.</param>
/// <param name="Mileage">Kilometers.</param>
/// <param name="Location">City and region.</param>
/// <param name="Description">Description.</param>
public sealed record ListingFacts(
    string? Make,
    string? Model,
    int? Year,
    decimal? Price,
    int? Mileage,
    string? Location,
    string? Description);

/// <summary>Listing fact messages and bounds.</summary>
public static class ListingRules
{
    /// <summary>Make field message.</summary>
    public const string MakeMessage = "Make must be 1 to 50 characters.";

    /// <summary>Model field message.</summary>
    public const string ModelMessage = "Model must be 1 to 50 characters.";

    /// <summary>Price field message.</summary>
    public const string PriceMessage = "Price must be greater than 0 and at most 99,999,999.99, with at most two decimal places.";

    /// <summary>Mileage field message.</summary>
    public const string MileageMessage = "Mileage must be a whole number of kilometers from 0 through 2,000,000.";

    /// <summary>Location field message.</summary>
    public const string LocationMessage = "Location must be 2 to 80 characters.";

    /// <summary>Description field message.</summary>
    public const string DescriptionMessage = "Description must be 1 to 2000 characters.";

    /// <summary>Problem detail when a listing is refused.</summary>
    public const string NotSaved = "The listing was not saved.";

    /// <summary>UTC calendar year after the current year.</summary>
    public static int MaxYear(IClock clock) => clock.UtcNow.Year + 1;

    /// <summary>Year field message for the clock's maximum year.</summary>
    public static string YearMessage(int maxYear) =>
        $"Year must be a whole number from 1900 through {maxYear}.";

    /// <summary>Adds a message for each fact that fails its bounds.</summary>
    public static void Validate(ValidationErrors errors, ListingFacts facts, IClock clock)
    {
        if ((facts.Make?.Trim().Length ?? 0) is < 1 or > 50)
        {
            errors.Add("make", MakeMessage);
        }

        if ((facts.Model?.Trim().Length ?? 0) is < 1 or > 50)
        {
            errors.Add("model", ModelMessage);
        }

        var maxYear = MaxYear(clock);
        if (facts.Year is not int year || year < 1900 || year > maxYear)
        {
            errors.Add("year", YearMessage(maxYear));
        }

        if (facts.Price is not decimal price
            || price <= 0
            || price > 99999999.99m
            || decimal.Round(price, 2, MidpointRounding.AwayFromZero) != price)
        {
            errors.Add("price", PriceMessage);
        }

        if (facts.Mileage is not int mileage || mileage < 0 || mileage > 2_000_000)
        {
            errors.Add("mileage", MileageMessage);
        }

        if ((facts.Location?.Trim().Length ?? 0) is < 2 or > 80)
        {
            errors.Add("location", LocationMessage);
        }

        if ((facts.Description?.Trim().Length ?? 0) is < 1 or > 2000)
        {
            errors.Add("description", DescriptionMessage);
        }
    }
}
