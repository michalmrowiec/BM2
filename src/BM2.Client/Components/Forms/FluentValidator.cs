using BM2.Client.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BM2.Client.Components.Forms;

/// <summary>
/// Validates an EditForm with the shared BM2.Contracts validator. Form models are mutable classes whose property
/// names match the request, so <see cref="ToRequest"/> maps the model to the validated request.
/// </summary>
public sealed class FluentValidator : ComponentBase, IDisposable
{
    private EditContext? _context;
    private ValidationMessageStore? _messages;

    [CascadingParameter]
    private EditContext? CurrentEditContext { get; set; }

    [Parameter, EditorRequired]
    public IValidator Validator { get; set; } = null!;

    [Parameter]
    public Func<object, object>? ToRequest { get; set; }

    /// <summary>Shows server-side errors (400/409) next to their fields until the field is edited.</summary>
    public void ShowServerErrors(ApiProblem problem)
    {
        if (_context is null || _messages is null)
        {
            return;
        }

        foreach (var (field, errors) in problem.Errors.Where(e => e.Key.Length > 0))
        {
            _messages.Add(_context.Field(field), errors);
        }

        _context.NotifyValidationStateChanged();
    }

    public void Dispose() => Detach();

    protected override void OnParametersSet()
    {
        if (CurrentEditContext is null)
        {
            throw new InvalidOperationException($"{nameof(FluentValidator)} must be placed inside an EditForm.");
        }

        if (ReferenceEquals(CurrentEditContext, _context))
        {
            return;
        }

        Detach();
        _context = CurrentEditContext;
        _messages = new ValidationMessageStore(_context);
        _context.OnValidationRequested += OnValidationRequested;
        _context.OnFieldChanged += OnFieldChanged;
    }

    private void OnValidationRequested(object? sender, ValidationRequestedEventArgs e)
    {
        _messages!.Clear();
        foreach (var error in Validate())
        {
            _messages.Add(_context!.Field(error.PropertyName), error.ErrorMessage);
        }

        _context!.NotifyValidationStateChanged();
    }

    private void OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        var field = e.FieldIdentifier;
        _messages!.Clear(field);
        foreach (var error in Validate().Where(error => error.PropertyName == field.FieldName))
        {
            _messages.Add(field, error.ErrorMessage);
        }

        _context!.NotifyValidationStateChanged();
    }

    private List<ValidationFailure> Validate()
    {
        var request = ToRequest?.Invoke(_context!.Model) ?? _context!.Model;
        return Validator.Validate(new ValidationContext<object>(request)).Errors;
    }

    private void Detach()
    {
        if (_context is not null)
        {
            _context.OnValidationRequested -= OnValidationRequested;
            _context.OnFieldChanged -= OnFieldChanged;
        }
    }
}
