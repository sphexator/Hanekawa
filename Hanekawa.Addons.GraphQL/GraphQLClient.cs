using Hanekawa.Rest;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hanekawa.Addons.GraphQL.Internal;

namespace Hanekawa.Addons.GraphQL
{
    public class GraphQLClient
    {
        private readonly RestClient restClient;

        public GraphQLClient(string url)
        {
            restClient = new RestClient(url);
        }

        /// <summary>
        ///     Query the endpoint with a raw function.
        /// </summary>
        /// <param name="query">GraphQL query</param>
        /// <param name="variables">Variables used in query</param>
        /// <returns>Object of type T converted</returns>
        public async Task<string> QueryAsync(string query, params GraphQLParameter[] variables)
        {
            return await InternalQueryAsync(CreateQueryJson(query, variables));
        }

        /// <summary>
        ///     Query the endpoint with a raw function and receive the json response, for parameters use $p0, $p1... to access your
        ///     variables
        /// </summary>
        /// <param name="query">GraphQL query</param>
        /// <param name="variables">Variables used in query</param>
        /// <returns>Json response</returns>
        public async Task<string> QueryAsync(string query, params object[] variables)
        {
            return await InternalQueryAsync(CreateQueryJson(query, variables));
        }

        /// <summary>
        ///     Query the endpoint with a raw function, for parameters use $p0, $p1... to access your variables
        /// </summary>
        /// <typeparam name="T">The output object serialized to</typeparam>
        /// <param name="query">GraphQL query</param>
        /// <param name="variables">Variables used in query</param>
        /// <returns>Object of type T converted</returns>
        public async Task<T> QueryAsync<T>(string query, params object[] variables)
        {
            return await InternalQueryAsync<T>(CreateQueryJson(query, variables));
        }

        /// <summary>
        ///     Query the endpoint with a raw function
        /// </summary>
        /// <typeparam name="T">The output object serialized to</typeparam>
        /// <param name="query">GraphQL query</param>
        /// <param name="variables">Variables used in query</param>
        /// <returns>Object of type T converted</returns>
        public async Task<T> QueryAsync<T>(string query, params GraphQLParameter[] variables)
        {
            return await InternalQueryAsync<T>(CreateQueryJson(query, variables));
        }

        /// <summary>
        ///     Query GraphQL string and receive json
        /// </summary>
        /// <param name="query">GraphQL query</param>
        /// <returns>Json response</returns>
        internal async Task<string> InternalQueryAsync(string query)
        {
            return (await restClient.PostAsync("", query)).Body;
        }

        /// <summary>
        ///     Query GraphQL string and receive serialized object
        /// </summary>
        /// <param name="query">GraphQL query</param>
        /// <returns>Response of type T</returns>
        internal async Task<T> InternalQueryAsync<T>(string query)
        {
            var response = await restClient.PostAsync<GraphQLQuery<T>>("", query);
            if (response.Success) return response.Data.Data;
            return default(T);
        }

        /// <summary>
        ///     Utility function to create queries for post messages
        /// </summary>
        /// <param name="query">base query</param>
        /// <param name="variables">variables from query</param>
        /// <returns>postable query</returns>
        private string CreateQueryJson(string query, params object[] variables)
        {
            var allVariables = new Dictionary<string, object>();

            for (var i = 0; i < variables.Length; i++) allVariables.Add($"p{i}", variables[i]);

            return CreateQueryJson(query, allVariables);
        }

        /// <summary>
        ///     Utility function to create queries for post messages
        /// </summary>
        /// <param name="query">base query</param>
        /// <param name="variables">variables from query</param>
        /// <returns>postable query</returns>
        private string CreateQueryJson(string query, params GraphQLParameter[] variables)
        {
            var allVariables = new Dictionary<string, object>();
            var queryArgs = new List<string>();

            var queryBase = "query(";

            for (var i = 0; i < variables.Length; i++)
            {
                queryArgs.Add($"$p{i}:{variables[i].ParamType}");
                allVariables.Add($"p{i}", variables[i].Value);
            }

            return CreateQueryJson($"{queryBase}{string.Join(",", queryArgs)}){{ {query} }}", allVariables);
        }

        /// <summary>
        ///     Utility function to create queries for post messages
        /// </summary>
        /// <param name="query">base query</param>
        /// <param name="variables">variables from query</param>
        /// <returns>postable query</returns>
        private string CreateQueryJson(string query, Dictionary<string, object> variables)
        {
            var serializedVariables = JsonConvert.SerializeObject(variables);
            return FormatQueryString(query, serializedVariables);
        }

        /// <summary>
        ///     Formats the query in a GraphQL-like json structure
        /// </summary>
        /// <param name="query">query string</param>
        /// <param name="variables">variable string</param>
        /// <returns>GraphQL-formatted json</returns>
        private string FormatQueryString(string query, string variables)
        {
            return $"{{ \"query\": \"{query}\", \"variables\": {variables} }}";
        }
    }
}