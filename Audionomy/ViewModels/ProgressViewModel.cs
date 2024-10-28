namespace Audionomy.ViewModels
{
    using System.Windows;

    public class ProgressViewModel
    {
        public int MaxValue { get; set; } = 0;

        public int Value { get; set; } = 0;

        public string CurrentAction { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public Visibility Visibility { get; set; } = Visibility.Hidden;

        public ProgressViewModel() { }

        public ProgressViewModel(int maxValue, int value, string currentAction, string summary)
        {
            CurrentAction = currentAction;
            Summary = summary;
            MaxValue = maxValue;
            Value = value;
            Visibility = Visibility.Visible;
        }
    }
}
