namespace BM2.Client.Services;

/// <summary>A failed API call. <see cref="Status"/> 0 means the server could not be reached.</summary>
public sealed record ApiProblem(int Status, string? Detail, IReadOnlyDictionary<string, string[]> Errors)
{
    public static ApiProblem Network { get; } = new(0, null, new Dictionary<string, string[]>());

    public bool IsNotFoundOrForbidden => Status is 403 or 404;

    public bool HasFieldErrors => Errors.Keys.Any(key => key.Length > 0);
}

public record ApiResult(ApiProblem? Problem)
{
    public bool Ok => Problem is null;
}

public sealed record ApiResult<T>(T? Value, ApiProblem? Problem) : ApiResult(Problem);
