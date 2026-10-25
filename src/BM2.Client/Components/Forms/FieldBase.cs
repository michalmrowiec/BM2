using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BM2.Client.Components.Forms;

/// <summary>Labelled form field wired to EditForm validation (ids for label, hint and error).</summary>
public abstract class FieldBase<TValue> : InputBase<TValue>
{
    private readonly string _generatedId = "f-" + Guid.NewGuid().ToString("N")[..8];

    [Parameter, EditorRequired]
    public string Label { get; set; } = "";

    [Parameter]
    public string? Hint { get; set; }

    protected string FieldId =>
        AdditionalAttributes?.TryGetValue("id", out var id) == true ? Convert.ToString(id, CultureInfo.InvariantCulture)! : _generatedId;

    protected string ErrorId => FieldId + "-error";

    protected string HintId => FieldId + "-hint";

    protected string? ErrorText
    {
        get
        {
            var messages = EditContext.GetValidationMessages(FieldIdentifier).ToList();
            return messages.Count == 0 ? null : string.Join(" ", messages);
        }
    }

    protected string? AriaInvalid => ErrorText is null ? null : "true";

    protected string? DescribedBy => ErrorText is not null ? ErrorId : Hint is not null ? HintId : null;
}
