namespace BM2.Client.Services;

public enum ToastKind
{
    Info,
    Success,
    Error,
}

public sealed record Toast(Guid Id, string Message, ToastKind Kind);

public sealed class ToastService
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(4);
    private readonly List<Toast> _items = [];

    public event Action? Changed;

    public IReadOnlyList<Toast> Items => _items;

    public void Show(string message, ToastKind kind = ToastKind.Info)
    {
        var toast = new Toast(Guid.NewGuid(), message, kind);
        _items.Add(toast);
        Changed?.Invoke();
        _ = DismissLaterAsync(toast.Id);
    }

    /// <summary>Generic failure; skipped while a specific error (read-only demo, rate limit) is already showing.</summary>
    public void ShowError()
    {
        if (!_items.Any(t => t.Kind == ToastKind.Error))
        {
            Show(Localization.Strings.Tr("Something went wrong. Try again."), ToastKind.Error);
        }
    }

    public void Dismiss(Guid id)
    {
        if (_items.RemoveAll(t => t.Id == id) > 0)
        {
            Changed?.Invoke();
        }
    }

    private async Task DismissLaterAsync(Guid id)
    {
        await Task.Delay(Lifetime);
        Dismiss(id);
    }
}
