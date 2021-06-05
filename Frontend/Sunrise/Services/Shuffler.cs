using System;
using System.Collections.Generic;
using System.Text;

namespace SunriseLauncher.Services
{
    public static class Shuffler
    {
        private static Random rnd = new Random();

        public static void Shuffle<T>(IList<T> list)
        {
            for (var i = list.Count; i > 0; i--)
            {
                var j = rnd.Next(0, i);
                var temp = list[0];
                list[0] = list[j];
                list[j] = temp;
            }
        }
    }
}
