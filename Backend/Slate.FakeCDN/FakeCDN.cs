using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Slate.FakeCDN;

Console.Title = "Development CDN (Not for production use)";
if (args.Any(a => a.Contains("--attachDebugger"))) Debugger.Break();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
    }).Build();

await host.RunAsync();
