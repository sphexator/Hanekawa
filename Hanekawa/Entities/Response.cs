namespace Hanekawa.Entities;

public class Response<T> where T : notnull
{
    public Response(T value)
    {
        Value = value;
        IsSuccess = true;
    }

    public Response(T value, bool isSuccess)
    {
        IsSuccess = isSuccess;
        Value = value;
    }

    public T Value { get; set; }

    public bool IsSuccess { get;  }
}