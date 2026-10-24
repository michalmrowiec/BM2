namespace BM2.IntegrationTests.Infrastructure;

/// <summary>The real clock shifted by an offset; reset before every test.</summary>
public sealed class TestTimeProvider : TimeProvider
{
    private TimeSpan _offset;

    public override DateTimeOffset GetUtcNow() => base.GetUtcNow() + _offset;

    public void Advance(TimeSpan by) => _offset += by;

    public void Reset() => _offset = TimeSpan.Zero;
}
