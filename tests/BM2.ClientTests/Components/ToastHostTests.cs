using BM2.Client.Components;
using BM2.Client.Services;
using BM2.ClientTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BM2.ClientTests.Components;

public class ToastHostTests : BunitContext
{
    [Fact]
    public void Toast_host_is_a_live_popover_brought_to_the_top_for_each_new_toast()
    {
        this.UseBm2Client();
        var module = JSInterop.SetupModule("./Components/ToastHost.razor.js");
        module.SetupVoid("raise", _ => true).SetVoidResult();
        var cut = Render<ToastHost>();
        var toasts = Services.GetRequiredService<ToastService>();

        cut.InvokeAsync(() => toasts.Show("Link copied."));

        var host = cut.Find(".toast-host");
        host.HasAttribute("popover").ShouldBeTrue();
        host.GetAttribute("aria-live").ShouldBe("polite");
        cut.WaitForAssertion(() => cut.Find(".toast").TextContent.ShouldContain("Link copied."));
        cut.WaitForAssertion(() => module.Invocations["raise"].Count.ShouldBe(2));
    }
}
