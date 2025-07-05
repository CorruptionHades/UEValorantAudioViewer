using System.Windows.Controls;
using UEValorantAudioViewer.utils;
using UEValorantAudioViewer.utils.audio;

namespace UEValorantAudioViewer;

public partial class LoadingScreen : UserControl
{
    private MainWindow _main;
    public LoadingScreen(MainWindow main)
    {
        InitializeComponent();
        _main = main;
        Loaded += async (_, __) =>
        {
            
            Shared.init();
            
            //   await Task.Delay(2000); // Simulate load
            await AudioService.getAudio();
            _main.ShowAudioBrowser();
        };
    }
}