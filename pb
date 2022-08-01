using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace pokerbot2
{
    public struct Card
    {
        public string Suit;
        public int Rank;
    }
    public class Hand
    {
        public int Score;
        public decimal TieBreaker;
        public static bool IsStraight(int s)
        {
            int lsb = s & -s;
            int normalized = s / lsb;
            return normalized == 31;

        }
        public static decimal GetTieBreaker(int[] ranks)
        {
            Array.Sort(ranks);
            Array.Reverse(ranks);
            foreach (int rank in ranks)
            {
                Console.WriteLine(rank);
            }
            int tiebreaker = ranks[0] << 16 |  ranks[1] << 12 | ranks[2] << 8 | ranks[3] << 4 | ranks[4] << 0;
            return tiebreaker;
        }
        public static int GetScore(int s, ulong v, int[] ranks, string[] suits)
        {
            if (Enumerable.SequenceEqual(ranks,new int[] {14,2,3,4,5 })){
                Console.WriteLine("Straight");
                return 5;
                
            }
            int score = 0;
            decimal cardType = v % 15;
            switch (cardType)
            {
                case 1:
                    score = 8;
                    Console.WriteLine("Four of a Kind");
                    break;
                case 10:
                    score = 7;
                    Console.WriteLine("Full House");
                    break;
                case 9:
                    score = 4;
                    Console.WriteLine("Three of a Kind");
                    break;
                case 7:
                    score = 3;
                    Console.WriteLine("Two Pairs");
                    break;
                case 6:
                    score = 2;
                    Console.WriteLine("One Pair");
                    break;
                case 5:
                    score = 1;
                    Console.WriteLine("High Card");
                    break;
                default:
                    break;
            }
            if (IsStraight(s) && suits.Distinct().Count() == 1)
            {
                Console.WriteLine("Straight Flush");
                score = 9;
            }
            else if (IsStraight(s))
            {
                Console.WriteLine("Straight");
                score = 5;

            }
            else if (suits.Distinct().Count() == 1)
            {
                Console.WriteLine("Flush");
                score = 6;
                if (Enumerable.SequenceEqual(ranks, new int[] { 10, 11, 12, 13, 14 })) {
                    Console.WriteLine("Royal Flush");
                    score = 10;
                }

            }
            return score;
        }
        public Hand(int[] ranks, string[] suits)
        {
            int s = 1 << ranks[0] | 1 << ranks[1] | 1 << ranks[2] | 1 << ranks[3] | 1 << ranks[4];
            ulong v = 0;
            Dictionary<int, int> instances = new Dictionary<int, int>();
            for (int i = 0; i < 5; i++)
            {
                int offset = 0;
                int rank = ranks[i];
                if ((s & 1 << rank) > 0)
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
                ulong addition = (ulong)Math.Pow(2, rank * 4);
                addition = addition << offset;
                v = v | addition;

            }
            v = v >> 1;
            Score = GetScore(s, v, ranks, suits);
            TieBreaker = GetTieBreaker(ranks);


        }
        public int CompareTo(Hand hand)
        {
            if (Score.CompareTo(hand.Score) != 0)
            {
                return Score.CompareTo(hand.Score);
            }
            return TieBreaker.CompareTo(hand.TieBreaker);
        }
    }
    
    class Program
    {


        public static Card[] MyCards = new Card[] { new Card { Suit = "S", Rank = 2 }, new Card { Suit = "S", Rank = 3 }, new Card { Suit = "S", Rank = 4 }, new Card { Suit = "S", Rank = 5 }, new Card { Suit = "S", Rank = 6 } };
       
       
        static void Main(string[] args)
        {
            Hand hand = new Hand(ExampleCards, MySuits);
           
           
        }
    }
}
