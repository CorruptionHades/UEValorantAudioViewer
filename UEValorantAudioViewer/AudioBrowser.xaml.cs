using System.Windows.Controls;
using System.Windows;
using UEValorantAudioViewer.utils;
using UEValorantAudioViewer.utils.audio;

namespace UEValorantAudioViewer;

public partial class AudioBrowser : UserControl
{
    public class AudioFile
    {
        public string DisplayName { get; set; }
        public string WemNumberedName { get; set; }
        public string WemNumberedPath { get; set; }
        public string UAssetSource { get; set; }
    }

    private List<AudioFile> allFiles;

    public AudioBrowser()
    {
        InitializeComponent();

        // Sample data
        allFiles = AudioService.audioFileList;

        AudioList.ItemsSource = allFiles;
        AudioList.DisplayMemberPath = "DisplayName";
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string filter = SearchBox.Text.ToLower();
        AudioList.ItemsSource = allFiles
            .Where(f => f.DisplayName.ToLower().Contains(filter))
            .ToList();
    }

    private void AudioList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = AudioList.SelectedItem as AudioFile;
        if (selected != null)
        {
            DetailName.Text = $"Name: {selected.DisplayName}";
            DetailNumber.Text = $"Numbered: {selected.WemNumberedName}";
            DetailUAsset.Text = $"Source: {selected.UAssetSource}";
        }
    }

    private void Extract_Click(object sender, RoutedEventArgs e)
    {
        var selected = AudioList.SelectedItems.Cast<AudioFile>().ToList();
        
        Console.WriteLine($"Extracting {selected.Count} audio files...");
        foreach (AudioFile audioFile in selected) {
            Console.WriteLine($"Extracting {audioFile.DisplayName} from {audioFile.UAssetSource}");
            AudioExtraction.Extract(audioFile);
        }
        
        MessageBox.Show($"Extracted {selected.Count} audio files to {Settings.settings.OutputFolder}", "Extraction Complete", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}