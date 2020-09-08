using System.IO;

namespace Hanekawa.HungerGames.Extensions
{
    internal static class StreamExtensions
    {
        internal static MemoryStream ToEditable(this Stream stream)
        {
            var toReturn = new MemoryStream();
            stream.CopyTo(toReturn);
            stream.Flush();
            return toReturn;
        }
    }
}
