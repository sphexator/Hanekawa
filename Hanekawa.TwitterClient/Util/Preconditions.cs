using System;
using System.Collections.Generic;
using System.Text;

namespace Hanekawa.TwitterClient.Util
{
    internal static class Preconditions
    {
        //Objects
        public static void NotNull<T>(T obj, string name, string msg = null) where T : class { if (obj == null) throw CreateNotNullException(name, msg); }
        
        private static ArgumentNullException CreateNotNullException(string name, string msg)
        {
            if (msg == null) return new ArgumentNullException(name);
            else return new ArgumentNullException(name, msg);
        }
    }
}
