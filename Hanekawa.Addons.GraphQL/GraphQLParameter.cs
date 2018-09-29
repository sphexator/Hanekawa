namespace Hanekawa.Addons.GraphQL
{
    public class GraphQLParameter
    {
        public GraphQLParameter(object value, string type = "String")
        {
            Value = value;
            ParamType = type;
        }

        public object Value { get; internal set; }

        public string ParamType { get; internal set; }
    }
}