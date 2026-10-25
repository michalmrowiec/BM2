using BM2.Client.Components;
using BM2.Contracts.Categories;
using Microsoft.AspNetCore.Components.Web;

namespace BM2.ClientTests.Components;

public class SegmentedControlTests : BunitContext
{
    private static readonly IReadOnlyList<(CategoryType Value, string Label)> TypeOptions =
        [(CategoryType.Expense, "Expenses"), (CategoryType.Income, "Income")];

    private CategoryType _selected = CategoryType.Expense;

    private IRenderedComponent<SegmentedControl<CategoryType>> RenderControl() =>
        Render<SegmentedControl<CategoryType>>(p => p
            .Add(x => x.Options, TypeOptions)
            .Add(x => x.Value, _selected)
            .Add(x => x.ValueChanged, value => _selected = value)
            .Add(x => x.Label, "Category type"));

    [Fact]
    public void Is_a_labelled_radio_group_with_selected_option_checked()
    {
        var cut = RenderControl();

        cut.Find("[role=radiogroup]").GetAttribute("aria-label").ShouldBe("Category type");
        cut.Find("[aria-checked=true]").TextContent.ShouldBe("Expenses");
    }

    [Fact]
    public void Click_selects_option()
    {
        var cut = RenderControl();

        cut.FindAll("[role=radio]")[1].Click();

        _selected.ShouldBe(CategoryType.Income);
    }

    [Fact]
    public void Arrow_right_selects_next_option()
    {
        var cut = RenderControl();

        cut.Find("[aria-checked=true]").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        _selected.ShouldBe(CategoryType.Income);
    }

    [Fact]
    public void Arrow_left_wraps_to_last_option()
    {
        var cut = RenderControl();

        cut.Find("[aria-checked=true]").KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        _selected.ShouldBe(CategoryType.Income);
    }

    [Fact]
    public void Only_selected_option_is_in_tab_order()
    {
        var cut = RenderControl();

        cut.FindAll("[role=radio]").Select(b => b.GetAttribute("tabindex")).ShouldBe(["0", "-1"]);
    }
}
