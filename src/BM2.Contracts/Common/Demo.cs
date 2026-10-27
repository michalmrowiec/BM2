namespace BM2.Contracts.Common;

/// <summary>The shared read-only demo account (Demo:Enabled on the server).</summary>
public static class Demo
{
    /// <summary>Role claim of the demo user; the server refuses its writes.</summary>
    public const string Role = "Demo";

    public const string ReadOnlyMessage = "The demo is read-only. Create an account to keep your own records.";
}
