using System;
using Demo.Core;

namespace Demo.App
{
    public class NumberGenerator : INumberGenerator
    {
        public int GenerateNumber()
        {
            Random r = new Random();
            return r.Next(0, 100);
        }
    }
}
