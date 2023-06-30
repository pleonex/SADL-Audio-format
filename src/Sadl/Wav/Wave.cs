using Yarhl.FileFormat;

namespace SceneGate.Ekona.Audio.Sadl.Wav;

public class Wave : IFormat
{
    public static string MagicHeader { get { return "RIFF"; } }
    public static string RiffFormat { get { return "WAVE"; } }

    public int Channels { get; set; }

    public int SampleRate { get; set; }

    public int BitsPerSample { get; set; }

    public int ByteRate => Channels * SampleRate * BitsPerSample / 8;

    public int FullSampleSize => Channels * BitsPerSample / 8;
}
