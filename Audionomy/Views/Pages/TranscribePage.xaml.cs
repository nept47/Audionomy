﻿namespace Audionomy.Views.Pages
{
    using Audionomy.ViewModels.Pages;
    using Wpf.Ui.Controls;

    /// <summary>
    /// Interaction logic for TranscribePage.xaml
    /// </summary>
    public partial class TranscribePage : INavigableView<TranscribeViewModel>
    {
        public TranscribePage(TranscribeViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }

        public TranscribeViewModel ViewModel { get; }
    }
}
