using System;
using Demo.Infra;

namespace Demo.WebApi
{
    public class NumberGenerator : INumberGenerator
    {
        public int GenerateNumber()
        {
            Random r = new Random();
            return r.Next(200, 300);
        }
    }
}
