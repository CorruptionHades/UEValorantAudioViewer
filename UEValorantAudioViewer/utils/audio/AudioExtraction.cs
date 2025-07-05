using System.IO;

namespace UEValorantAudioViewer.utils.audio;

public class AudioExtraction {

    public static String Extract(AudioBrowser.AudioFile audioFile) {
        return Extract(audioFile.WemNumberedPath, audioFile.DisplayName);
    }
    
    public static String Extract(String pkgPath, String wavName) {
        String filePath = Settings.settings.OutputFolder + "/" + wavName.Replace("ShooterGame/Content/WwiseAudio/", "");
        String wemFilePath = filePath + ".wem";
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        
        Console.WriteLine("Extracting " + pkgPath + " to " + wemFilePath);
        byte[] data = Shared.Provider.SaveAsset(pkgPath);
        File.WriteAllBytes(wemFilePath, data);
        Console.WriteLine("Extracted to " + wemFilePath);
        
        String wavPath = filePath + (filePath.EndsWith(".wav") ? "" : ".wav");

        String cmd = $"-o \"{wavPath}\" \"{wemFilePath}\"";
        Console.WriteLine("Running command: " + cmd);

        System.Diagnostics.Process p = System.Diagnostics.Process.Start("vgm/vgmstream-cli.exe", cmd);
        //  p.StartInfo.RedirectStandardOutput = true;
        p.WaitForExit();

        // delete the temporary .wem file
        File.Delete(wemFilePath);
        
        Console.WriteLine("Wav file at \"" + wavPath.Replace("/", "\\") + "\"");
        
        return wavPath;
    }
}