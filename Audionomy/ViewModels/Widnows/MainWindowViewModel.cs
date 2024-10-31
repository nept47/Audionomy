using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Controls;

namespace Audionomy.ViewModels.Widnows
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _applicationTitle = "Audionomy";

        [ObservableProperty]
        private ObservableCollection<object> _menuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Audio Synthesis",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Speaker220 },
                ToolTip = "Convert Text to WAV"    ,
                TargetPageType = typeof(Views.Pages.SpeechSynthesizePage)
            },
            new NavigationViewItem()
            {
                Content = "Text Transcription",
                Icon = new SymbolIcon { Symbol = SymbolRegular.DocumentText20 },
                ToolTip = "Transcribe WAV File to Text",
                TargetPageType = typeof(Views.Pages.TranscribePage),              
            }
        };

        [ObservableProperty]
        private ObservableCollection<object> _footerMenuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "Settings",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
                ToolTip = "Settings",
                TargetPageType = typeof(Views.Pages.SettingsPage)
            }
        };

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new()
        {
            new MenuItem { Header = "Home", Tag = "tray_home" }
        };
    }
}
