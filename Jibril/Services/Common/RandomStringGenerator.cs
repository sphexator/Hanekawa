using System;
using System.Collections.Generic;
using System.Text;

namespace Jibril.Services.Common
{
    public class RandomStringGenerator
    {
        public static string StringGenerator()
        {
            string randomString = "";
            string input = "abcdefghijklmnopqrstuvwxyz0123456789";
            char ch;
            Random rand = new Random();
            for (int i = 0; i < 8; i++)
            {
                ch = input[rand.Next(0, input.Length)];
                randomString += ch;
            }
            return randomString;
        }
    }
}
