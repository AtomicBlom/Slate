using System;
using GameWarden;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
