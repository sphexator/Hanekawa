using System.IO;

namespace Hanekawa.Extensions
{
    public static class StreamExtensions
    {
        public static MemoryStream ToEditable(this Stream stream)
        {
            var toReturn = new MemoryStream();
            stream.CopyTo(toReturn);
            stream.Flush();
            return toReturn;
        }
    }
}
