using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace PhotoGalleryAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        webBuilder.UseKestrel(
                           options =>
                           {
                               options.ConfigureEndpointDefaults(listneOption => listneOption.UseConnectionLogging());
                               options.Listen(IPAddress.Any, 6000);
                               options.Listen(IPAddress.Any, 6001,
                                    listenOptions =>
                                    {
                                        listenOptions.UseHttps("/https/localhost.pfx", "PhotoGalleryHTTPSCertificate");
                                    });
                           }
                      );


                    };
                    webBuilder.UseStartup<Startup>();
                });
    }
}
