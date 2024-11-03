namespace Audionomy.ViewModels
{
    using Wpf.Ui.Controls;

    public class InfoMessageModel
    {
        public string Message { get; set; } = string.Empty;

        public bool IsOpen { get; set; } = false;

        public InfoBarSeverity Severity { get; set; } = InfoBarSeverity.Informational;

        public InfoMessageModel() { }

        public InfoMessageModel(string message, InfoBarSeverity severity)
        {
            Message = message;
            Severity = severity;
            IsOpen = true;
        }
    }
}