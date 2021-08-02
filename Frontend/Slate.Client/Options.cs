using System;
using CommandLine;

namespace Slate.Client
{
    public class Options
    {
        [Option('a', "AuthServer", Required = true, HelpText = "The URL of the Authorization Server")]
        public Uri AuthServer { get; set; } = null!;

        [Option('g', "GameServer", Required = true, HelpText = "Host name of the Game Server")]
        public string GameServer { get; set; } = null!;

        [Option('p', "GameServerPort", Required = false, HelpText = "The server's port")]
        public int GameServerPort { get; set; } = 4000;

        [Option("log-seq", Required = false, HelpText = "The address of a https://getseq.net/ logging server")]
        public string? SeqUrl { get; set; } = null;

        [Option("log-console", Required = false, HelpText = "Opens a console window for logging")]
        public bool LogToConsole { get; set; } = false;
    }
}