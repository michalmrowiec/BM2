using System.Net;
using System.Net.Http.Json;
using BM2.Contracts.Profile;
using BM2.IntegrationTests.Infrastructure;

namespace BM2.IntegrationTests.Features;

public class MeTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Anonymous_request_gets_401_without_redirect()
    {
        using var client = Factory.CreateAnonymousClient();

        var response = await client.GetAsync("/api/me", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Headers.Location.ShouldBeNull();
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
    }

    [Fact]
    public async Task Authenticated_user_gets_own_email()
    {
        var user = await CreateUserAsync("ala@example.com");
        using var client = Factory.CreateClientAs(user.Id);

        var me = await client.GetFromJsonAsync<MeResponse>("/api/me", TestContext.Current.CancellationToken);

        me.ShouldBe(new MeResponse(user.Id, "ala@example.com"));
    }

    [Fact]
    public async Task Cookie_for_deleted_user_gets_401()
    {
        using var client = Factory.CreateClientAs(Guid.NewGuid());

        var response = await client.GetAsync("/api/me", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
