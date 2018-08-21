namespace Hanekawa.GraphQL
{
    public class GraphQLParameter
    {
        public object Value { get; internal set; }

        public string ParamType { get; internal set; }

        public GraphQLParameter(object value, string type = "String")
        {
            Value = value;
            ParamType = type;
        }
    }
}
