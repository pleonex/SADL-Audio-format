using Yarhl.FileFormat;
using Yarhl.IO;

namespace SceneGate.Ekona.Audio.Sadl.Wav;

public class WaveBinaryConverter :
    IConverter<Wave, BinaryFormat>,
    IConverter<BinaryFormat, Wave>
{
    public Wave Convert(BinaryFormat source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var reader = new DataReader(source.Stream);

        // Read RIFF header
        if (reader.ReadString(4) != Wave.MagicHeader) {
            throw new FormatException("Invalid header.");
        }

        reader.ReadUInt32(); // File size - 8

        if (reader.ReadString(4) != Wave.RiffFormat) {
            throw new FormatException("Invalid riff format.");
        }

        // Sub-chunk 'fmt'
        if (reader.ReadString(4) != "fmt ") {
            throw new FormatException("Invalid fmt chunk.");
        }

        reader.ReadUInt32(); // sub-chunk size
        ushort audioCodec = reader.ReadUInt16();

        var format = new Wave();
        format.Channels = reader.ReadUInt16();
        format.SampleRate = reader.ReadInt32();
        reader.ReadInt32();  // Byte rate
        reader.ReadUInt16(); // Full sample size
        format.BitsPerSample = reader.ReadUInt16();

        // Sub-chunk 'data'
        if (reader.ReadString(4) != "data") {
            throw new FormatException("Invalid data chunk");
        }

        uint dataSize = reader.ReadUInt32();
        var audioStream = new DataStream(source.Stream, source.Stream.Position, dataSize);
        // format.Decoder  = (audioCodec == 1) ? new PcmDecoder(format, audioStream) : null;

        return format;
    }

    public BinaryFormat Convert(Wave source)
    {
        ArgumentNullException.ThrowIfNull(source);
        // var audioStream = source.Decoder.RawStream;

        var binary = new BinaryFormat();
        var writer = new DataWriter(binary.Stream);
        writer.Write(Wave.MagicHeader);
        // writer.Write((uint)(36 + audioStream.Length));
        writer.Write(Wave.RiffFormat);

        // Sub-chunk 'fmt'
        writer.Write("fmt ");
        writer.Write((uint)16); // Sub-chunk size
        writer.Write((ushort)1); // Audio format
        writer.Write((ushort)source.Channels);
        writer.Write(source.SampleRate);
        writer.Write(source.ByteRate);
        writer.Write((ushort)source.FullSampleSize);
        writer.Write((ushort)source.BitsPerSample);

        // Sub-chunk 'data'
        writer.Write("data");
        // writer.Write((uint)(audioStream.Length));
        // audioStream.WriteTo(binary.Stream);

        return binary;
    }
}
