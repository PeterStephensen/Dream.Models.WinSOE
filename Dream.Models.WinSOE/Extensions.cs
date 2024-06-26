﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dream.Models.WinSOE
{
    
    public static class Extensions
    {
        #region NextGaussian()
        /// <summary>
        /// Returns a random gaussian number 
        /// </summary>
        /// <param name="mean">Mean</param>
        /// <param name="stdDev">Standard deviation</param>
        /// <returns></returns>
        public static double NextGaussian(this Random random, double mean, double stdDev)
        {
            double u1 = random.NextDouble();                                                         //these are uniform(0,1) random doubles
            double u2 = random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);    //random normal(0,1)
            double randNormal = mean + stdDev * randStdNormal;                                       //random normal(mean,stdDev^2)

            return randNormal;

        }
        #endregion

        #region NextPareto()
        /// <summary>
        /// Returns a random pareto distributed number  
        /// </summary>
        /// <param name="m_min">Scale parameter (minimum value)</param>
        /// <param name="k">Shape parameter</param>
        /// <param name="censor_top">Share of top events that are censored away</param>
        /// <returns></returns>
        public static double NextPareto(this Random random, double m_min, double k, double censor_top=0)
        {
            return m_min / Math.Pow(random.NextDouble(censor_top, 1), 1.0/k);
        }
        #endregion

        #region NextInteger()
        /// <summary>
        /// Returns Floor(d) and adds 1 with probability d - Floor(d)
        /// </summary>
        /// <param name="random"></param>
        /// <param name="d"></param>
        /// <returns>Integer</returns>
        /// 
        public static int NextInteger(this Random random, double d)
        {
            double r = d - Math.Floor(d);
            int n = (int)Math.Floor(d);

            if (random.NextDouble() < r)
                n++;

            return n;
        }
        #endregion

        #region NextDouble(min, max)
        /// <summary>
        /// Returns a random integer between d_min and d_max
        /// </summary>
        /// <param name="random"></param>
        /// <param name="d_min"></param>
        /// <param name="d_max"></param>
        /// <returns></returns>
        public static double NextDouble(this Random random, double d_min, double d_max)
        {
            return d_min + (d_max - d_min) * random.NextDouble();
        }
        #endregion

        #region NextEvent()
        /// <summary>
        /// Returns true with probablility p
        /// </summary>
        /// <param name="random"></param>
        /// <param name="p">Probability that event happens</param>
        /// <returns>Bool</returns>
        public static bool NextEvent(this Random random, double p)
        {
            if(random.NextDouble() < p)
                return true;
            else 
                return false;
        }
        #endregion

        #region WriteLineTab()
        /// <summary>
        /// Write tab seperated list to StreamWriter
        /// </summary>
        /// <param name="o">List of arguments</param>
        public static void WriteLineTab(this StreamWriter sw, params object[] o)
        {
            sw.WriteLine(string.Join('\t', o));
        }
        #endregion

        #region Median()
        /// <summary>
        /// Returns median from double list
        /// </summary>
        /// <param name="list">List of doubles</param>
        /// <returns>Median as double</returns>
        public static double Median(this List<double> list)
        {
            List<double> l = list.OrderBy(x => x).ToList();
            int count = list.Count;
            int halfIndex = count / 2;
            double median;
            if (count % 2 == 0)
                median = (l[halfIndex - 1] + l[halfIndex]) / 2;
            else
                median = l[halfIndex];

            return median;
        }
        #endregion

        #region Shuffle()
        public static void Shuffle<T>(this Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }
        #endregion

        #region Last elements in List and double array
        /// <summary>
        /// Returns the last n elements in a list
        /// </summary>
        /// <param name="n">Number of elements to return</param>
        public static List<T> Last<T>(this List<T> list, int n)
        {
            // Ensure n is not greater than the number of elements in the list
            if (n > list.Count)
                throw new ArgumentException("Value of n exceeds the number of elements in the list.");

            // Use LINQ Skip method to skip the first count - n elements, then take the last n elements
            return list.Skip(list.Count - n).ToList();
        }
        /// <summary>
        /// Returns the last n elements in a double array
        /// </summary>
        /// <param name="n">Number of elements to return</param>
        public static double[] Last(this double[] d, int n)
        {
            // Ensure n is not greater than the number of elements in the list
            if (n > d.Length)
                throw new ArgumentException("Value of n exceeds the number of elements in the list.");

            double[] x = new double[n];
            Array.Copy(d, d.Length - n, x, 0, n);

            return x;
        }
        #endregion

    }
}
