using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BM2.Client.Services.Notification;

public interface IUiService
{
    Task<bool> ConfirmDeleteAsync(string title = "Delete Confirmation", string message = "Are you sure you want to delete this item?");


    void ShowDeleteSuccess(string deletedItemName);
    void ShowSuccess(string message);
    void ShowError(string message);
    void ShowWarning(string message);
    void ShowInfo(string message);
}

public class UiService : IUiService
{
    private readonly IDialogService _dialogService;
    private readonly ISnackbar _snackbar;

    public UiService(IDialogService dialogService, ISnackbar snackbar)
    {
        _dialogService = dialogService;
        _snackbar = snackbar;
    }

    public async Task<bool> ConfirmDeleteAsync(string title, string message)
    {
        bool? result = await _dialogService.ShowMessageBox(
            title,
            message,
            yesText: "Delete",
            noText: "Cancel"
        );

        return result ?? false;
    }

    private Action<SnackbarOptions> snackbarOptions = config =>
    {
        config.OnClick = snck => Task.CompletedTask;
    };

    public void ShowDeleteSuccess(string deletedItemName) => _snackbar.Add(new MarkupString($"Deleted <b>{deletedItemName}</b>"), Severity.Success, snackbarOptions);

    public void ShowSuccess(string message) => _snackbar.Add(message, Severity.Success);
    public void ShowError(string message) => _snackbar.Add(message, Severity.Error);
    public void ShowWarning(string message) => _snackbar.Add(message, Severity.Warning);
    public void ShowInfo(string message) => _snackbar.Add(message, Severity.Info);
}