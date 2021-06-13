using SunriseLauncher.Models;
using System;
using System.Diagnostics;
using System.IO;

namespace SunriseLauncher.Services
{
    public class Launcher
    {
        public void Launch(Server server, LaunchOption launch)
        {
            try
            {
                if (launch == null)
                    return;

                var fullpath = Path.Combine(server.InstallPath, launch.LaunchPath);

                var process = new Process();
                process.StartInfo.WorkingDirectory = Path.GetDirectoryName(fullpath);
                process.StartInfo.FileName = fullpath;
                process.StartInfo.Arguments = launch.Args;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                process.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception while launching: {0}", ex.Message);
            }
        }
    }
}
