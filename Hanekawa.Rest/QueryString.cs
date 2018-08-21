using System.Collections.Generic;
using System.Linq;

namespace Hanekawa.Rest
{
    public class QueryString
    {
        private readonly Dictionary<string, object> queryArgs = new Dictionary<string, object>();

        public string Query
            => "?" + string.Join("&", queryArgs.Select(x => $"{x.Key}={x.Value.ToString()}"));

        public void Add(string key, object value)
        {
            queryArgs.Add(key, value);
        }
    }
}