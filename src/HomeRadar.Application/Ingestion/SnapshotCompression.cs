using System.IO.Compression;
using System.Text;

namespace HomeRadar.Application.Ingestion;

public static class SnapshotCompression
{
    public static byte[] Compress(string body)
    {
        using var output = new MemoryStream();
        using (var brotli = new BrotliStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            brotli.Write(Encoding.UTF8.GetBytes(body));
        }

        return output.ToArray();
    }

    public static string Decompress(byte[] compressed)
    {
        using var input = new MemoryStream(compressed);
        using var brotli = new BrotliStream(input, CompressionMode.Decompress);
        using var reader = new StreamReader(brotli, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
