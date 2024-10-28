namespace Audionomy.ViewModels
{
    using Wpf.Ui.Controls;

    public class ErrorViewModel
    {
        public string Message { get; set; } = string.Empty;

        public bool IsOpen { get; set; } = false;

        public InfoBarSeverity Severity { get; set; } = InfoBarSeverity.Informational;

        public ErrorViewModel() { }

        public ErrorViewModel(string message, InfoBarSeverity severity)
        {
            Message = message;
            Severity = severity;
            IsOpen = true;
        }
    }
}