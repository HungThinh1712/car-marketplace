namespace CarMarketplace.Api.Exceptions;

/// <summary>A request failed field rules and nothing was saved.</summary>
public sealed class FieldValidationException : Exception
{
    /// <summary>Creates a validation failure whose detail is safe to show.</summary>
    public FieldValidationException(string clientDetail, IReadOnlyDictionary<string, string[]> errors)
        : base(clientDetail)
    {
        ClientDetail = clientDetail;
        Errors = errors;
    }

    /// <summary>Safe sentence for the problem detail.</summary>
    public string ClientDetail { get; }

    /// <summary>Field names and the sentences the page can show.</summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }
}

/// <summary>The requested record does not exist.</summary>
public sealed class NotFoundException : Exception
{
    /// <summary>Creates a not-found failure with a safe detail sentence.</summary>
    public NotFoundException(string clientDetail)
        : base(clientDetail)
    {
        ClientDetail = clientDetail;
    }

    /// <summary>Safe sentence for the problem detail.</summary>
    public string ClientDetail { get; }
}

/// <summary>The caller is signed in but is not allowed to do this.</summary>
public sealed class ForbiddenException : Exception
{
    /// <summary>Creates a forbidden failure with a safe detail sentence.</summary>
    public ForbiddenException(string clientDetail)
        : base(clientDetail)
    {
        ClientDetail = clientDetail;
    }

    /// <summary>Safe sentence for the problem detail.</summary>
    public string ClientDetail { get; }
}

/// <summary>The request conflicts with the current record.</summary>
public sealed class ConflictException : Exception
{
    /// <summary>Creates a conflict failure with a safe detail sentence.</summary>
    public ConflictException(string clientDetail)
        : base(clientDetail)
    {
        ClientDetail = clientDetail;
    }

    /// <summary>Safe sentence for the problem detail.</summary>
    public string ClientDetail { get; }
}

/// <summary>The caller is not signed in.</summary>
public sealed class UnauthorizedException : Exception
{
    /// <summary>Creates an unauthorized failure with a safe detail sentence.</summary>
    public UnauthorizedException(string clientDetail)
        : base(clientDetail)
    {
        ClientDetail = clientDetail;
    }

    /// <summary>Safe sentence for the problem detail.</summary>
    public string ClientDetail { get; }
}
