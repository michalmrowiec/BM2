using BM2.Client.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BM2.ClientTests;

public class ShellComponentTests : BunitContext
{
    [Fact]
    public void Theme_toggle_is_a_labelled_button_wired_to_theme_script()
    {
        var cut = Render<ThemeToggle>();

        var button = cut.Find("button");
        button.GetAttribute("type").ShouldBe("button");
        button.HasAttribute("data-theme-toggle").ShouldBeTrue();
        button.GetAttribute("aria-label").ShouldBe("Change theme");
    }

    [Fact]
    public void Bottom_nav_has_main_sections_and_quick_add()
    {
        var cut = Render<BottomNav>();

        cut.FindAll("a").Select(a => a.GetAttribute("href")).ShouldBe(["", "records", "records/new", "stats", "more"]);
        cut.Find("a[href='records/new']").GetAttribute("aria-label").ShouldBe("Add record");
    }

    [Fact]
    public void Bottom_nav_marks_current_section()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("records");

        var cut = Render<BottomNav>();

        cut.Find("a[href='records']").GetAttribute("aria-current").ShouldBe("page");
        cut.Find("a[href='']").HasAttribute("aria-current").ShouldBeFalse();
    }

    [Fact]
    public void Quick_add_page_does_not_mark_records_as_current()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("records/new");

        var cut = Render<BottomNav>();

        cut.Find("a[href='records']").HasAttribute("aria-current").ShouldBeFalse();
    }
}
