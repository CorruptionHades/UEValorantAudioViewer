using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UEValorantAudioViewer.utils;
using UEValorantAudioViewer.utils.killbanner;
using Color = System.Drawing.Color;

namespace UEValorantAudioViewer;

public partial class KillBannerViewer : UserControl {

    public class KillBannerFile {
        public string DisplayName { get; set; }
        public string UAssetPath { get; set; }
        
        public Color PrimaryColor { get; set; }
        
        public string? TexturePath { get; set; }
        public string? SliceDefault { get; set; }
        public string? SliceHover { get; set; }
        public int? SliceRadius { get; set; }
        public int? HeadshotOffset { get; set; }
        public string? DefaultTexturePath { get; set; }
        public string? BackgroundFramePath { get; set; }
    }
    
    private List<KillBannerFile> allKillBanners;
    
    public KillBannerViewer() {
        InitializeComponent();
        
        
        Loaded += async (_, __) =>
        {
            
            loadKillBanners();
            KillbannerList.ItemsSource = allKillBanners;
            KillbannerList.DisplayMemberPath = "DisplayName";
        };
    }

    private async void loadKillBanners() {
        Shared.init();
        
        allKillBanners = await KillbannerService.GetKillBanners();
    }

    private void KillbannerList_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
        var selected = KillbannerList.SelectedItem as KillBannerFile;
        if (selected != null) {
            Info.Text = $"Selected Kill Banner: {selected.DisplayName}" +
                        $"\nUAsset Path: {selected.UAssetPath}" +
                        $"\nPrimary Color: {selected.PrimaryColor}" +
                        $"\nTexture Path: {selected.TexturePath ?? "N/A"}" +
                        $"\nSlice Default: {selected.SliceDefault ?? "N/A"}" +
                        $"\nSlice Hover: {selected.SliceHover ?? "N/A"}" +
                        $"\nSlice Radius: {selected.SliceRadius?.ToString() ?? "N/A"}" +
                        $"\nHeadshot Offset: {selected.HeadshotOffset?.ToString() ?? "N/A"}" +
                        $"\nDefault Texture Path: {selected.DefaultTexturePath ?? "N/A"}" +
                        $"\nBackground Frame Path: {selected.BackgroundFramePath ?? "N/A"}";
        }
    }

    private void Extract_Click(object sender, RoutedEventArgs e) {
        // get current selected
        var selected = KillbannerList.SelectedItem as KillBannerFile;
        if (selected == null) {
            MessageBox.Show("Please select a kill banner to extract.");
            return;
        }
        
        KillbannerService.ExportedKillBanner? exported = KillbannerService.extractKillBanner(selected);
        if (exported != null) {
            RenderKillBanner(exported, selected);
        } else {
            MessageBox.Show("Failed to extract Kill Banner.");
        }
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) {
        string filter = SearchBox.Text.ToLower();
        KillbannerList.ItemsSource = allKillBanners
            .Where(f => f.DisplayName.ToLower().Contains(filter))
            .ToList();
    }
    
    private void RenderKillBanner(KillbannerService.ExportedKillBanner exported, KillBannerFile file) {
        KillbannerRenderCanvas.Children.Clear();

        if (exported.BackgroundFramePath != null) {
            renderImg(exported.BackgroundFramePath, 0);
        }
        if (exported.TexturePath != null) {
            renderImg(exported.TexturePath, 0);
        }
        if (exported.SliceDefault != null) {
            renderImg(exported.SliceDefault, file.SliceRadius ?? 0, 90);
        }
        if (exported.SliceHover != null) {
            renderImg(exported.SliceHover, file.SliceRadius ?? 0);
        }
        if (exported.DefaultTexturePath != null) {
            renderImg(exported.DefaultTexturePath, 0);
        }
    }

    private void renderImg(string filepath, double offset, int rotation = 0) {
        var bitmapImage = new BitmapImage(new Uri(filepath, UriKind.RelativeOrAbsolute));

        var image = new Image {
            Source = bitmapImage,
            Width = bitmapImage.Width,
            Height = bitmapImage.Height,
        };

        // Calculate offsets to center the texture
        double offsetX = (KillbannerRenderCanvas.Width - bitmapImage.Width) / 2;
        double offsetY = (KillbannerRenderCanvas.Height - bitmapImage.Height) / 2 - (offset / 2);
        
        Canvas.SetLeft(image, offsetX);
        Canvas.SetTop(image, offsetY);
        KillbannerRenderCanvas.Children.Add(image);
    }
}