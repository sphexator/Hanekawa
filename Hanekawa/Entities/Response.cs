namespace Hanekawa.Entities;

public class Response<T> where T : notnull
{
    public Response(T data) => Data = data;
    public T Data { get; set; }
}