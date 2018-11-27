namespace Hanekawa
{
    public class Program
    {
        private static void Main()
        {
            new HanekawaBot().StartAsync().GetAwaiter().GetResult();
        }
    }
}