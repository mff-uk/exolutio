using System;

namespace EvoX.SupportingClasses
{
    public static class RandomGenerator
    {
        private static Random r;


        static RandomGenerator()
        {
            r = new Random(DateTime.Now.Millisecond);
        }

        public static uint Next()
        {
            return (uint) r.Next();
        }

        public static uint Next(uint max)
        {
            return (uint) r.Next((int) max);
        }

        public static int Next(int min, int max)
        {
            return r.Next((int) min, (int) max);
        }

        public static int Next(int min, int max, int elementOccurrences)
        {
            return r.Next(min, Math.Max(max - 2 * elementOccurrences, min));
        }

        public static bool Toss()
        {
            return r.Next(2) == 1; 
        }

        public static bool Toss(int forTrue, int forFalse)
        {
            return r.Next(forTrue + forFalse) < forTrue;
        }

        public static DateTime RandomDateTime()
        {
            return new DateTime((int) Next(1990, 2009), (int) Next(1, 13), (int) Next(1, 28), (int) Next(0, 24), (int) Next(0, 24), (int) Next(0, 60));
        }
    }
}