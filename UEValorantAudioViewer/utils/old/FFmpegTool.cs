namespace UEValorantAudioViewer.utils.old;

public class FFmpegTool {

    public static string combineAudio(List<String> files, string outputName) {  
        // ffmpeg -i audio1.wav -i audio2.wav -filter_complex "[0:a][1:a]amix=inputs=2:duration=longest" -c:a pcm_s16le output.wav
        String outputFilePath = Settings.settings.OutputFolder + "/" + outputName + ".wav";
        
        Console.WriteLine("Combining " + files.Count + " audio files into " + outputFilePath);
        
        String inputFiles = String.Join(" ", files.Select(f => $"-i \"{f}\""));
        String filterComplex = $"-filter_complex \"{String.Join("", files.Select((f, i) => $"[{i}:a]")).TrimEnd(':')}amix=inputs={files.Count}:duration=longest\"";
        String cmd = $"{inputFiles} {filterComplex} -c:a pcm_s16le \"{outputFilePath}\"";
        
        Console.WriteLine("Running command: " + cmd);
        System.Diagnostics.Process p = System.Diagnostics.Process.Start("ffmpeg.exe", cmd);
        
        p.OutputDataReceived += (sender, args) => {
            if (args.Data != null && args.Data.Contains("overwrite")) {
                p.StandardInput.WriteLine("y");
            }
        };
        
        p.WaitForExit();
        
        return outputFilePath;
    }

    public static string concatAudio(List<string> files, string outputName) {
        // ffmpeg -i "concat:audio1.wav|audio2.wav" -c copy output.wav
        String outputFilePath = Settings.settings.OutputFolder + "/" + outputName + ".wav";
        
        Console.WriteLine("Concatenating " + files.Count + " audio files into " + outputFilePath);
        
        String inputFiles = $"-i \"concat:{String.Join("|", files)}\"";
        String cmd = $"{inputFiles} -c copy \"{outputFilePath}\"";
        
        Console.WriteLine("Running command: " + cmd);
        System.Diagnostics.Process p = System.Diagnostics.Process.Start("ffmpeg.exe", cmd);
        
        p.OutputDataReceived += (sender, args) => {
            if (args.Data != null && args.Data.Contains("overwrite")) {
                p.StandardInput.WriteLine("y");
            }
        };
        
        p.WaitForExit();
        
        return outputFilePath;
    }
}