using System;

namespace Jibril.Services.Common
{
    public class RandomStringGenerator
    {
        public static string StringGenerator()
        {
            var randomString = "";
            var input = "abcdefghijklmnopqrstuvwxyz0123456789";
            char ch;
            var rand = new Random();
            for (var i = 0; i < 8; i++)
            {
                ch = input[rand.Next(0, input.Length)];
                randomString += ch;
            }
            return randomString;
        }
    }
}