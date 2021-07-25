using System;
using System.Collections.Generic;
using System.Text;

namespace CastIron.Engine
{
    public static class RandomExtensions
    {
        public static double NextGaussianDouble(this Random rand, double mean, double standardDeviation)
        {
            var u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
            var u2 = 1.0 - rand.NextDouble();
            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                                Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            return mean + standardDeviation * randStdNormal; //random normal(mean,standardDeviation^2)
        }
    }
}