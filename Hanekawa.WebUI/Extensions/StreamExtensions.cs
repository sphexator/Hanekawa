using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using Hanekawa.WebUI.Exceptions;

namespace Hanekawa.WebUI.Extensions
{
    public static class StreamExtensions
    {
        public static MemoryStream ToEditable(this Stream stream, double? mbLimit = null)
        {
            var toReturn = new MemoryStream();
            if (mbLimit == null)
            {
                stream.CopyTo(toReturn);
                toReturn.Flush();
                stream.Dispose();
            }
            else
            {
                var bufferSize = Buffer(stream);
                var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    toReturn.Write(buffer, 0, read);
                    if (mbLimit != null && (toReturn.Length / 1024f) / 1024f > mbLimit)
                        throw new HanaCommandException($"File size is above the limit ({mbLimit} MB)");
                }
                toReturn.Flush();
                stream.Dispose();
            }
            return toReturn;
        }

        private static int Buffer(Stream stream)
        {
            var bufferSize = 81920;

            if (!stream.CanSeek) return bufferSize;
            var length = stream.Length;
            var position = stream.Position;
            if (length <= position) bufferSize = 1;
            else
            {
                var remaining = length - position;
                if (remaining > 0) bufferSize = (int)Math.Min(bufferSize, remaining);
            }

            return bufferSize;
        }

        private static readonly Dictionary<FileType, byte[]> KnownFileHeaders = new Dictionary<FileType, byte[]>
        {
            { FileType.Jpeg, new byte[]{ 0xFF, 0xD8 }}, // JPEG
            { FileType.Gif, new byte[]{ 0x47, 0x49, 0x46 }}, // GIF
            { FileType.Png, new byte[]{ 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }}, // PNG
        };

        public static FileType GetKnownFileType(this MemoryStream stream)
        {
            ReadOnlySpan<byte> data = stream.ToArray().AsSpan();
            foreach (var (fileType, bytes) in KnownFileHeaders)
            {
                if (data.Length < bytes.Length) continue;
                var slice = data[..bytes.Length];
                if (slice.SequenceEqual(bytes))
                {
                    return fileType;
                }
            }

            return FileType.Unknown;
        }
	}
    
    public enum FileType
    {
        Unknown,
        Jpeg,
        Bmp,
        Gif,
        Png,
        Pdf
    }
}
