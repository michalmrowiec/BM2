using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace BM2.Client.Services.Notification
{
    public interface IAlertService
    {
        void Subscribe(Action<MarkupString, Severity> showAction);
        void ShowAlert(MarkupString message, Severity severity = Severity.Error);
    }

    public class AlertService : IAlertService
    {
        private Action<MarkupString, Severity>? _onShow;

        public void Subscribe(Action<MarkupString, Severity> showAction)
        {
            _onShow = showAction;
        }

        public void ShowAlert(MarkupString message, Severity severity = Severity.Error)
        {
            _onShow?.Invoke(message, severity);
        }
    }
}
