using BM2.Client.Services;

namespace BM2.ClientTests.Components;

public class FluentValidatorTests : BunitContext
{
    [Fact]
    public void Shows_validator_message_under_field()
    {
        var cut = Render<WalletFormHost>();

        cut.Find("form").Submit();

        cut.Find(".field__error").TextContent.ShouldBe("Enter a name.");
        cut.Find("input").GetAttribute("aria-invalid").ShouldBe("true");
        cut.Instance.Submitted.ShouldBeFalse();
    }

    [Fact]
    public void Error_is_linked_to_input_for_screen_readers()
    {
        var cut = Render<WalletFormHost>();

        cut.Find("form").Submit();

        var input = cut.Find("input");
        cut.Find(".field__error").Id.ShouldBe(input.GetAttribute("aria-describedby"));
        cut.Find("label").GetAttribute("for").ShouldBe(input.Id);
    }

    [Fact]
    public void Valid_form_submits()
    {
        var cut = Render<WalletFormHost>();

        cut.Find("input").Change("Home");
        cut.Find("form").Submit();

        cut.Instance.Submitted.ShouldBeTrue();
        cut.FindAll(".field__error").ShouldBeEmpty();
    }

    [Fact]
    public void Shows_server_errors_and_clears_them_on_edit()
    {
        var cut = Render<WalletFormHost>();
        cut.Find("input").Change("Home");

        cut.InvokeAsync(() => cut.Instance.Validator!.ShowServerErrors(
            new ApiProblem(409, null, new Dictionary<string, string[]> { ["Name"] = ["Taken"] })));

        cut.Find(".field__error").TextContent.ShouldBe("Taken");

        cut.Find("input").Change("Family");

        cut.FindAll(".field__error").ShouldBeEmpty();
    }
}
