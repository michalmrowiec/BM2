using System.Net;
using System.Net.Http.Json;
using BM2.Client.Components;
using BM2.Contracts.Profile;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BM2.ClientTests;

public class MeGreetingTests : BunitContext
{
    [Fact]
    public void Shows_email_of_current_user()
    {
        UseApi(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new MeResponse(Guid.NewGuid(), "demo@bm2.test")),
        });

        var cut = Render<MeGreeting>();

        cut.WaitForAssertion(() => cut.Markup.ShouldContain("demo@bm2.test"));
    }

    [Fact]
    public void Redirects_to_login_when_session_is_no_longer_valid()
    {
        UseApi(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));

        Render<MeGreeting>();

        var navigation = (BunitNavigationManager)Services.GetRequiredService<NavigationManager>();
        navigation.History.ShouldContain(entry => entry.Uri.Contains("Account/Login") && entry.Options.ForceLoad);
    }

    [Fact]
    public void Shows_error_when_api_fails()
    {
        UseApi(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var cut = Render<MeGreeting>();

        cut.WaitForAssertion(() => cut.Find("[role=alert]").TextContent.ShouldContain("Could not load"));
    }

    [Fact]
    public void Shows_error_when_network_is_unavailable()
    {
        UseApi(_ => throw new HttpRequestException("offline"));

        var cut = Render<MeGreeting>();

        cut.WaitForAssertion(() => cut.Find("[role=alert]").TextContent.ShouldContain("Could not load"));
    }

    private void UseApi(Func<HttpRequestMessage, HttpResponseMessage> respond) =>
        Services.AddSingleton(new HttpClient(new StubHandler(respond)) { BaseAddress = new Uri("http://localhost/") });

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }
}
