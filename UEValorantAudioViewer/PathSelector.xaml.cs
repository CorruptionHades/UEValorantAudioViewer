using System.Windows;
using System.Windows.Controls;
using UEValorantAudioViewer.utils;

namespace UEValorantAudioViewer;

public partial class PathSelector : UserControl
{
    private MainWindow _main;
    public PathSelector(MainWindow main)
    {
        InitializeComponent();
        _main = main;
        PathBox.Text = Settings.settings.PaksFolder;
        AesBox.Text = Settings.settings.AesKey;
        OutFolder.Text = Settings.settings.OutputFolder;
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        // Stub: File dialog
        PathBox.Text = "H:\\Programme\\Valorant\\Riot Games\\VALORANT\\live\\ShooterGame\\Content\\Paks";
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        Save();
 
        _main.ShowLoadingScreen();
    }

    private void Confirm_Killbanner(object sender, RoutedEventArgs e) {
        Save();
 
        _main.ShowKillBanner();
    }

    private void Save() {
        Settings.settings.PaksFolder = PathBox.Text;
        Settings.settings.AesKey = AesBox.Text;
        Settings.settings.OutputFolder = OutFolder.Text;
        Settings.SaveSettings();
    }
}