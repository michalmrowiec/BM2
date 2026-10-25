using System.Globalization;

namespace BM2.ClientTests.Infrastructure;

/// <summary>Switches the current culture for one test and restores it afterwards.</summary>
public sealed class CultureScope : IDisposable
{
    private readonly CultureInfo _previous = CultureInfo.CurrentCulture;

    public CultureScope(string name) => CultureInfo.CurrentCulture = new CultureInfo(name);

    public void Dispose() => CultureInfo.CurrentCulture = _previous;
}
