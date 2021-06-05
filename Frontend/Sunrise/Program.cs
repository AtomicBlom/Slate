using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Logging.Serilog;
using Avalonia.ReactiveUI;

namespace SunriseLauncher
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            using (var fileout = new FileStream("./log.txt", FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(fileout))
            {
                Console.SetOut(writer);
                writer.AutoFlush = true;

                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
        }


       // => BuildAvaloniaApp()
       //     .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug()
                .UseReactiveUI();
    }
}
