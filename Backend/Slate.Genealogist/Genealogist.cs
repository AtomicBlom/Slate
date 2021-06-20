using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Slate.Genealogist;
using Slate.Genealogist.Stores;

Console.Title = "Genealogist (Identity Server)";
if (args.Any(a => a.Contains("--attachDebugger"))) Debugger.Break();


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Verbose)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Verbose)
    .MinimumLevel.Override("System", LogEventLevel.Verbose)
    .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Verbose)
    .WriteTo.Seq("http://localhost:5341")
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
    .CreateLogger();

try
{
    Log.Information("Starting host...");
    var host = CreateHostBuilder(args).Build();
    var databaseMigration = host.Services.GetService<IMigrateDatabase>();
    await databaseMigration.Migrate();
    host.Run();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly.");
    Console.ReadLine();
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
    