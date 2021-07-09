using System;
using System.Diagnostics;
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
                .WithParsed<Options>(o =>
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

            using var game = new RudeEngineGame(options);

            game.Run();
        }
    }
}
