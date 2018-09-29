using Newtonsoft.Json;

namespace Hanekawa.Addons.GraphQL.Internal
{
    internal class GraphQLQuery<T>
    {
        [JsonProperty("data")] internal T Data;
    }
}