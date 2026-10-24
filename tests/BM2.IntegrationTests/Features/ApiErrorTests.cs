using System.Net;
using BM2.IntegrationTests.Infrastructure;

namespace BM2.IntegrationTests.Features;

public class ApiErrorTests(IntegrationFixture fixture) : IntegrationTest(fixture)
{
    [Fact]
    public async Task Unknown_api_route_returns_problem_details()
    {
        using var client = Factory.CreateAnonymousClient();

        var response = await client.GetAsync("/api/does-not-exist", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
    }
}
