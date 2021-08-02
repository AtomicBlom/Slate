using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using CommandLine;
using Microsoft.Xna.Framework;
using MLEM.Misc;

namespace Slate.Client
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            MlemPlatform.Current = new MlemPlatform.DesktopGl<TextInputEventArgs>((w, c) => w.TextInput += c);

            Options? options = null;

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    options = o;
                })
                .WithNotParsed(errors =>
                {
                    foreach (var error in errors)
                    {
                        Console.WriteLine(error);
                    }

                    throw new Exception("Unable to start application");

                });

            Debug.Assert(options != null);

            if (options.LogToConsole)
            {
                Win32.AllocConsole();
            }

            using var game = new RudeEngineGame(options);

            game.Run();
        }
    }

    internal static class Win32
    {
        /// <summary>
        /// allocates a new console for the calling process.
        /// </summary>
        /// <returns>If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero.
        /// To get extended error information, call Marshal.GetLastWin32Error.</returns>
        [DllImport("kernel32", SetLastError = true)]
        internal static extern bool AllocConsole();
    }

    public interface IUserLogEnricher
    {
        string UserId { set; }
        string CharacterId { set; }

        void Reset();
    }
}
