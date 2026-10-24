using BM2.Client.Services;
using BM2.Client.State;
using Microsoft.Extensions.DependencyInjection;

namespace BM2.ClientTests.Infrastructure;

public static class TestServices
{
    /// <summary>Client services wired to the stub API; JS interop answers loosely (localStorage returns null).</summary>
    public static StubApi UseBm2Client(this BunitContext context, StubApi? api = null)
    {
        api ??= new StubApi();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddSingleton(api.CreateClient());
        context.Services.AddBm2Client();
        return api;
    }
}
