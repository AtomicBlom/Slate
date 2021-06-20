using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Slate.GameWarden;

if (args.Any(a => a.Contains("--attachDebugger"))) Debugger.Break();

Console.Title = "Game Warden (Player Server)";

WebHost.CreateDefaultBuilder(args)
    .ConfigureKestrel(options =>
    {
        var configuration = options.ApplicationServices.GetService<IConfiguration>() ??
                            throw new Exception("Configuration was not available");
        
        if (!int.TryParse(configuration["port"], out var port))
        {
            port = 4000;
        }
        
        options.ListenAnyIP(port, listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http2;
        });
    })
    .UseStartup<Startup>()
    .Build()
    .Run();
