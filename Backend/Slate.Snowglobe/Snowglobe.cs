using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Slate.Snowglobe;

if (args.Any(a => a.Contains("--AttachDebugger")))
{
    if (!Debugger.IsAttached)
    {
        Debugger.Launch();
    }
    else
    {
        Debugger.Break();
    }
}

Console.Title = "Game Warden (Player Server)";

WebHost.CreateDefaultBuilder(args)
    .ConfigureKestrel(options =>
    {
        var configuration = options.ApplicationServices.GetService<IConfiguration>() ??
                            throw new Exception("Configuration was not available");

        if (!int.TryParse(configuration["port"], out var port))
        {
            port = 0;
        }

        options.ListenAnyIP(port, listenOptions =>
        {
            listenOptions.Protocols = HttpProtocols.Http2;
        });
    })
    .UseStartup<Startup>()
    .Build()
    .Run();