namespace Audionomy.ViewModels
{
    using Wpf.Ui.Controls;

    public class InfoMessageModel
    {
        public string Message { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool IsOpen { get; set; } = false;
        public bool IsClosable { get; set; } = true;

        public InfoBarSeverity Severity { get; set; } = InfoBarSeverity.Informational;

        public InfoMessageModel() { }

        public InfoMessageModel(string message, InfoBarSeverity severity, bool isClosable = true)
        {
            Message = message;
            Severity = severity;
            IsOpen = true;
            IsClosable = isClosable;
        }

        public InfoMessageModel(string title, string message, InfoBarSeverity severity, bool isClosable = true)
        {
            Message = message;
            Title = title;
            Severity = severity;
            IsOpen = true;
            IsClosable = isClosable;
        }
    }
}