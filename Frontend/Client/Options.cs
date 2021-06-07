using System;
using CommandLine;

namespace Client
{
    public class Options
    {
        [Option('a', "AuthServer", Required = true, HelpText = "The URL of the Authorization Server")]
        public Uri AuthServer { get; set; }

        [Option('g', "GameServer", Required = true, HelpText = "Host name of the Game Server")]
        public string GameServer { get; set; }

        [Option('p', "GameServerPort", Required = false, HelpText = "The server's port")]
        public int GameServerPort { get; set; } = 4000;
    }
}