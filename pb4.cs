using System;
using System.Collections.Generic;
using System.Linq;

namespace PokerBot
{
   
    class Program
    {
        public static int Compare(long full, int solo, long full2, int solo2)
        {
            if (GetScore(full, solo).CompareTo(GetScore(full2, solo2)) != 0){
                return GetScore(full, solo).CompareTo(GetScore(full2, solo2));
            }
            else
            {
                return GetTieBreaker(full >> 1).CompareTo(GetTieBreaker(full2 >> 1));
            }
        }
        public static bool isStraight(int solo)
        {
            int lsb = solo &= -solo;
            int normalized = solo / lsb;
            return normalized == 31;

        }
        public static int GetScore(long full, int solo)
        {
            if (solo == 16444)
            {
                return 5;
            }
            switch ((full >> 1) % 15)
            {
                case 1:
                    return 8;
                case 10:
                    return 7;
                case 9:
                    return 4;
                case 7:
                    return 3;
                case 6:
                    return 2;
                case 5:
                    return 1;
                default:
                    break;
            }
            if (isStraight(solo) && ((full & (1 << 0)) != 0))
            {
                return 9;
            }
            else if (isStraight(solo))
            {
                return 5;
            }
            else if ((full & (1 << 0)) != 0){
                if (solo == 31744)
                {
                    return 10;
                }
                return 6;
            }
            return 0;
        }
        public static long GetTieBreaker(long full)
        {   int one = GetHighestRank(full);
            int two = GetHighestRank(full ^ (1 << one));
            int three = GetHighestRank(full ^ (1 << two));
            int four = GetHighestRank(full ^ (1 << three));
            int five = GetHighestRank(full ^ (1 << four));
            long tiebreaker = one << 16 | two << 12 | three << 8 | four << 4 | five << 0;
            return tiebreaker;
        }


        public static int GetHighestRank(long full)
        {
            if (full == 0)
                return 0;

            int pos = 0;
            full = full / 2;

            while (full != 0)
            {
                full = full / 2;
                pos++;
            }

            return pos;
        }
        public static void ParseAsBits(int[] ranks, string[] suits,  out long full, out int solo)
        {
            solo = 0;
           for (int i = 0; i < ranks.Length; i++)
            {
                solo |= 1 << ranks[i];
            }
            Dictionary<int, int> instances = new Dictionary<int, int>();
            full = 0;
            for (int i = 0; i < 5; i++)
            {
                int offset = 0;
                int rank = ranks[i];
                if ((solo & 1 << rank) > 0)
                {
                    if (!instances.ContainsKey(rank))
                    {
                        instances.Add(rank, 1);
                    }
                    else
                    {
                        instances[rank] = instances[rank] + 1;
                    }
                    offset = instances[rank];
                }
                long addition = (long)Math.Pow(2, rank * 4);
                addition = addition << offset;
                full |= addition;
            }
            full = full >> 1;
            if (suits.Distinct().Count() == 1)
            {
                full |= (1 << 0);
            }
        }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }
    }
}
