using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nougat;

namespace Nougat
{
    class Program
    {
        static void Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder();
            var loggerFactory = new LoggerFactory()
                .AddConsole(LogLevel.Debug);

            configBuilder.AddCommandLine(args);

            var config =  configBuilder.Build();
            var projRoot = config.GetValue<string>("proj:root");
            var nugetSvr = config.GetValue<string>("nuget:server");

            if (projRoot == null && nugetSvr == null)
            {
                PrintOptions();
                return;   
            }
            
            if (projRoot == null)
            {
                PrintOptions();
                return;
            }
            
            NougatCli.Create(projRoot, loggerFactory, nugetSvr)
                .Run()
                .Wait();         
        }

        private static void PrintOptions()
        {
            Console.WriteLine("[REQUIRED] --proj:root - the root directory to search for projects (absolute path).");
            Console.WriteLine("[OPTIONAL] --nuget:server - the Nuget server to search.");
        }
    } 
}