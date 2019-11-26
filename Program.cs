// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Modified by Jack Butts for non-commercial purposes (https://www.github.com/buttsj)
namespace WestervilleFoodBot
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// Main program
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
