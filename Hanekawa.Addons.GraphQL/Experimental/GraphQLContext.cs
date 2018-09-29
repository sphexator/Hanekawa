namespace Hanekawa.Addons.GraphQL.Experimental
{
    public class GraphQLContext
    {
        public static GraphQLContext Create<T>() where T : GraphQLContext, new()
        {
            var t = new T();

            // TODO: initialize

            return t;
        }
    }
}