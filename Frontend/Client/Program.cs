using System;
using System.Diagnostics;
using CommandLine;

namespace Client
{
    public class Options
    {
        [Option('a', "AuthServer", Required = true, HelpText = "The URL of the Authorization Server")]
        public Uri AuthServer { get; set; }

        [Option('g', "GameServer", Required = true, HelpText = "Host name of the Game Server")]
        public string GameServer { get; set; }

        [Option('s', "StablePort", Required = false, HelpText = "The stable port of the server")]
        public int StablePort { get; set; } = 4000;

        [Option('u', "UnstablePort", Required = false, HelpText = "The unstable port of the server")]
        public int UnstablePort { get; set; } = 4001;
    }

    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Options? options = null;

            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    options = o;
                })
                .WithNotParsed((errors) =>
                {
                    foreach (var error in errors)
                    {
                        Console.WriteLine(error);
                    }

                    throw new Exception("Unable to start application");

                });

            Debug.Assert(options != null);

            using (var game = new RudeEngineGame(options))
                game.Run();
        }
    }
}
