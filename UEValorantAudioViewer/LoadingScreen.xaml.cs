using System.Windows.Controls;
using UEValorantAudioViewer.utils;

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
            
            Service.init();
            
            //   await Task.Delay(2000); // Simulate load
            await Service.getAudio();
            _main.ShowAudioBrowser();
        };
    }
}