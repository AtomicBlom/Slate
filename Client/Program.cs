using System;

namespace StopMMOping
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using (var game = new RudeEngineGame())
                game.Run();
        }
    }
}
