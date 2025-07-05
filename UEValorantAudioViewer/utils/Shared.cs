using System.IO;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Versions;

namespace UEValorantAudioViewer.utils;

public static class Shared {
    
    public static DefaultFileProvider Provider;
    
    public static void init() {

        Provider = new DefaultFileProvider(Settings.settings.PaksFolder, SearchOption.AllDirectories,
            new VersionContainer(EGame.GAME_Valorant), StringComparer.Ordinal);
        
        Provider.Initialize(); // will scan the archive directory for supported file extensions
        Provider.SubmitKey(new FGuid(), new FAesKey(Settings.settings.AesKey));
    }
}