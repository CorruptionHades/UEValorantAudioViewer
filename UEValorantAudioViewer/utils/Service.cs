using System.IO;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.FileProvider.Objects;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Versions;

namespace UEValorantAudioViewer.utils;

public static class Service {

    private static String[] folderPaths = {
        "ShooterGame/Content/WwiseAudio/Events"
    };
    
    public static DefaultFileProvider Provider;
    public static List<AudioBrowser.AudioFile> audioFileList = new();

    public static void init() {

        Provider = new DefaultFileProvider(Settings.settings.PaksFolder, SearchOption.AllDirectories,
            new VersionContainer(EGame.GAME_Valorant), StringComparer.Ordinal);
        
        Provider.Initialize(); // will scan the archive directory for supported file extensions
        Provider.SubmitKey(new FGuid(), new FAesKey(Settings.settings.AesKey));
    }

    public static Task<List<AudioBrowser.AudioFile>> getAudio() {
        
        //region Get all .wem audio files
        Console.WriteLine("Searching for .wem files...");
        Dictionary<string, GameFile> wemFiles = new();
        
        foreach (var file in Provider.Files) {
            if (file.Key.EndsWith(".wem")) {
                wemFiles.Add(file.Key, file.Value);
            }
        }
        Console.WriteLine("Found " + wemFiles.Count + " .wem files.");
        //endregion
        
        
        //region Event assets
        Console.WriteLine("Searching for event assets...");
        var eventAssets = new List<string>();
        
        foreach (var file in Provider.Files) {
            foreach (string folderPath in folderPaths) {
                if (file.Key.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase) && (file.Key.EndsWith(".uasset")))
                {
                    eventAssets.Add(file.Key);
                }
            }
        }
        Console.WriteLine("Found " + eventAssets.Count + " event assets.");
        //endregion

        Console.WriteLine("Loading event name maps...");
        var eventNameMaps = new Dictionary<string, Dictionary<string, string>>();
        
        //region Load event name maps
        foreach (var eventAsset in eventAssets) {
            var nameMap = LoadNameMap(eventAsset);
            if (nameMap.Count > 0) {
                eventNameMaps.Add(eventAsset, nameMap);
            }
        }
        Console.WriteLine("Loaded " + eventNameMaps.Count + " event name maps.");
        
        //region map wem files to wav files
        Console.WriteLine("Mapping .wem files to .wav files...");
        
        // go through the uasset files and map the wem files to wav files
        int count = 0;
        foreach (var eventNameMap in eventNameMaps) {
            var wemToWavMap = eventNameMap.Value;
            
            Console.WriteLine($"Processing {count++}/{eventNameMaps.Count} - {eventNameMap.Key}");
            
            foreach (var wemFile in wemFiles) {
                if (wemToWavMap.TryGetValue(wemFile.Key.Replace("ShooterGame/Content/WwiseAudio/", ""), out var wavFile)) {
                    audioFileList.Add(new AudioBrowser.AudioFile {
                        WemNumberedName = wemFile.Key.Replace("ShooterGame/Content/WwiseAudio/", ""),
                        WemNumberedPath = wemFile.Key,
                        DisplayName = wavFile,
                        UAssetSource = eventNameMap.Key
                    });
                }
            }
        }
        Console.WriteLine("Mapped " + audioFileList.Count + " audio files.");
        
        //endregion

        return Task.FromResult(audioFileList);
    }
    
    private static Dictionary<string, string> LoadNameMap(String pkgPath) {
        var pkg = Provider.LoadPackage(pkgPath);
        
        // Extract the NameMap array
        var nameMap = pkg.NameMap
            .Select(n => n.Name)
            .ToList();
        
        var wemToWavMap = new Dictionary<string, string>();
        
        var wemFiles = nameMap
            .Where(n => n.EndsWith(".wem"))
            .ToList();
        
        var wavFiles = nameMap
            .Where(n => n.EndsWith(".wav"))
            .ToList();
        
        for (int i = 0; i < wemFiles.Count; i++) {
            String wav;

            try {
                wav = wavFiles[i];
            }
            catch (ArgumentOutOfRangeException e) {
                wav = wemFiles[i];
            }

            wemToWavMap.Add(wemFiles[i], wav);
        }
        
        return wemToWavMap;
    }
}