using Newtonsoft.Json;

namespace Hanekawa.GraphQL.Internal
{
    internal class GraphQLQuery<T>
    {
        [JsonProperty("data")]
        internal T Data = default(T);
    }
}
