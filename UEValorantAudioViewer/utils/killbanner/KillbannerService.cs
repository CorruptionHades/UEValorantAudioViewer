using System.Drawing;
using System.IO;
using CUE4Parse_Conversion;
using CUE4Parse_Conversion.Textures;
using CUE4Parse.FileProvider.Objects;
using CUE4Parse.UE4.Assets.Exports.Texture;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UEValorantAudioViewer.utils.killbanner;

public static class KillbannerService {

    private static String[] folderPaths = {
        "ShooterGame/Content/UI/InGame/KillBanner/"
    };
    
    
    public static Task<List<KillBannerViewer.KillBannerFile>> GetKillBanners() {
        
        //region Get all killbanner data
        Console.WriteLine("Searching for .uasset files...");
        List<string> killbannerDataFiles = new();
        
        foreach (var file in Shared.Provider.Files) {
            if (file.Key.EndsWith(".uasset") && file.Key.Contains("KillBannerData")) {
                killbannerDataFiles.Add(file.Key);
            }
        }
        Console.WriteLine("Found " + killbannerDataFiles.Count + " killbanner files.");
        //endregion
        
        List<KillBannerViewer.KillBannerFile> killbanners = new();
        
        //region Load killbanner files
        Console.WriteLine("Loading killbanner files...");

        foreach (var filePath in killbannerDataFiles) {
            var killBannerFile = LoadKillBannerFile(filePath);
            if (killBannerFile != null) {
                killbanners.Add(killBannerFile);
            }
        }
        Console.WriteLine("Loaded " + killbanners.Count + " killbanner files.");
        //endregion
        

        return Task.FromResult(killbanners);
    }
    
    public static KillBannerViewer.KillBannerFile? ParseKillBannerFile(string json)
    {
        var jsonObject = JArray.Parse(json);

        // Find the object with the "Properties" key
        var properties = jsonObject.FirstOrDefault(obj => obj["Properties"] != null)?["Properties"]?["KillBannerData"];
        if (properties == null) return null;

        try {
            return new KillBannerViewer.KillBannerFile
            {
                DisplayName = jsonObject.FirstOrDefault(obj => obj["Name"] != null)?["Name"]?.ToString(),
                UAssetPath = jsonObject.FirstOrDefault(obj => obj["ClassDefaultObject"] != null)?["ClassDefaultObject"]?["ObjectPath"]?.ToString(),
                PrimaryColor = properties["PrimaryColor_2_E5FCE8F2449464E9BE54588D1A1DDA81"] != null
                    ? ColorTranslator.FromHtml("#" + properties["PrimaryColor_2_E5FCE8F2449464E9BE54588D1A1DDA81"]["Hex"]?.ToString())
                    : Color.White,
                TexturePath = properties["KillWheel-TXT_16_FEBDF04D4D730BD70C7995B402C44EF2"]?["ObjectPath"]?.ToString(),
                SliceDefault = properties["KillWheel_Slice_Default_18_A2EF828C4325ABF6030B19962A75A149"]?["ObjectPath"]?.ToString(),
                SliceHover = properties["KillWheel_Slice_Hover_20_493CD45B4598268ADD2FA380C4354AB8"]?["ObjectPath"]?.ToString(),
                SliceRadius = properties["KillWheel_Slice_Radius_23_93B498D8455F7ECBDF0367ADF77B93E3"]?.ToObject<int>(),
                HeadshotOffset = properties["Badge_HeadshotOffset_34_46CE41534C94C72E84BC8195F948FA17"] != null
                    ? (int?)properties["Badge_HeadshotOffset_34_46CE41534C94C72E84BC8195F948FA17"]["X"]?.ToObject<int>()
                    : null,
                DefaultTexturePath = properties["Badge_Default_TXT_69_50160A5A4C36A394A83226BD4C657FF8"]?["ObjectPath"]?.ToString(),
                BackgroundFramePath = properties["BackgroundFrame_TXT_12_AE806B364880513036A646929D3B6758"]?["ObjectPath"]?.ToString()
            };
        }
        catch (Exception e) {
            Console.WriteLine("Error parsing Kill Banner File: " + e.Message);
            return null;
        }
    }
    
    private static KillBannerViewer.KillBannerFile? LoadKillBannerFile(string pkgPath) {
        
        var pkg = Shared.Provider.LoadPackage(pkgPath);
        var exports = pkg.GetExports();
        var fullJson = JsonConvert.SerializeObject(exports, Formatting.Indented);

        return ParseKillBannerFile(fullJson);
    }

    public static ExportedKillBanner? extractKillBanner(KillBannerViewer.KillBannerFile file) {

        ExportedKillBanner exported = new();
        
        var options = new ExporterOptions {
            TextureFormat = ETextureFormat.Png,
        };
        
        string exportDir = Settings.settings.OutputFolder;
        
        if (string.IsNullOrEmpty(exportDir)) {
            Console.WriteLine("Output folder is not set.");
            return null;
        }
        
        Console.WriteLine("Exporting kill banner: " + file.DisplayName);
        int exportCount = 0;
        string folder = Path.Combine("KillBanners", file.DisplayName);
        Directory.CreateDirectory(Path.Combine(exportDir, folder));
        
        //region Export textures (needs to be this order)
        exported.BackgroundFramePath = tryExport(exportDir, folder, options, file.BackgroundFramePath).FirstOrDefault();
        exported.TexturePath = tryExport(exportDir, folder, options, file.TexturePath).FirstOrDefault();
        exported.SliceDefault = tryExport(exportDir, folder, options, file.SliceDefault).FirstOrDefault();
        exported.SliceHover = tryExport(exportDir, folder, options, file.SliceHover).FirstOrDefault();
        exported.DefaultTexturePath = tryExport(exportDir, folder, options, file.DefaultTexturePath).FirstOrDefault();
        //endregion
        
        return exported;
    }

    private static List<string> tryExport(string exportDir, string folder, ExporterOptions options, string path) {
        
        List<string> exportedPaths = new List<string>();
        int exportCount = 0;
        
        if (!string.IsNullOrEmpty(path)) {
            var obj = Shared.Provider.LoadPackageObject(path.Replace(".0", ""));

            if (obj is UTexture2D tex) {
                var paths = SaveTexture(exportDir, folder, tex, ETexturePlatform.DesktopMobile, options, ref exportCount);
                exportedPaths.AddRange(paths);
            }
        }
        
        return exportedPaths;
    }
    
    private static List<string> SaveTexture(string exportDir, string folder, UTexture texture, ETexturePlatform platform, ExporterOptions options, ref int exportCount) {

        List<string> paths = new List<string>();
        
        var bitmaps = new[] { texture.Decode() };
        
        switch (texture)
        {
            case UTexture2DArray textureArray:
                bitmaps = textureArray.DecodeTextureArray(platform);
                break;
            case UTextureCube:
                bitmaps[0] = bitmaps[0]?.ToPanorama();
                break;
        }

        foreach (var bitmap in bitmaps)
        {
            if (bitmap is null) continue;
            
            var bytes = bitmap.Encode(options.TextureFormat, 1);
            var fileName = $"{texture.Name}.png";

            paths.Add(WriteToFile(exportDir, folder, fileName, bytes.ToArray(), $"{fileName} ({bitmap.Width}x{bitmap.Height})", ref exportCount));
        }

        return paths;
    }
    
    private static string WriteToFile(string exportDir, string folder, string fileName, byte[] bytes, string logMessage, ref int exportCount)
    {
        Directory.CreateDirectory(Path.Combine(exportDir, folder));
        File.WriteAllBytesAsync(Path.Combine(exportDir, folder, fileName), bytes);

        return Path.Combine(exportDir, folder, fileName);
    }

    public class ExportedKillBanner {
        public string? TexturePath { get; set; }
        public string? SliceDefault { get; set; }
        public string? SliceHover { get; set; }
        public string? DefaultTexturePath { get; set; }
        public string? BackgroundFramePath { get; set; }
    }

}