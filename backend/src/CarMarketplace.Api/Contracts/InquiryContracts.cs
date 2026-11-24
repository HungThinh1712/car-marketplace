namespace CarMarketplace.Api.Contracts;

/// <summary>Payload for one inquiry. There is no reply field.</summary>
public sealed record CreateInquiryRequest
{
    /// <summary>Buyer name, 1 to 80 characters.</summary>
    public string? Name { get; init; }

    /// <summary>Buyer email.</summary>
    public string? Email { get; init; }

    /// <summary>Optional phone.</summary>
    public string? Phone { get; init; }

    /// <summary>Message, 10 to 1000 characters.</summary>
    public string? Message { get; init; }
}

/// <summary>Confirmation that names the car. It does not echo the message as a thread.</summary>
/// <param name="Id">Inquiry id.</param>
/// <param name="ListingId">Listing id.</param>
/// <param name="Make">Listing make.</param>
/// <param name="Model">Listing model.</param>
/// <param name="Year">Listing year.</param>
/// <param name="SubmittedAt">UTC time the inquiry was stored.</param>
public sealed record InquiryCreatedResponse(
    string Id,
    string ListingId,
    string Make,
    string Model,
    int Year,
    DateTimeOffset SubmittedAt);

/// <summary>One inquiry as read by the listing owner.</summary>
/// <param name="Id">Inquiry id.</param>
/// <param name="Name">Buyer name.</param>
/// <param name="Email">Buyer email.</param>
/// <param name="Phone">Buyer phone, omitted when it was not provided.</param>
/// <param name="Message">Buyer message.</param>
/// <param name="SubmittedAt">UTC time the inquiry was stored.</param>
public sealed record InquiryResponse(
    string Id,
    string Name,
    string Email,
    string? Phone,
    string Message,
    DateTimeOffset SubmittedAt);

/// <summary>Inquiries for one listing, newest first.</summary>
/// <param name="Items">Inquiry rows.</param>
public sealed record InquiryListResponse(IReadOnlyList<InquiryResponse> Items);
