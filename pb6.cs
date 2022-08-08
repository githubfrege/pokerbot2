using System;
using System.Collections.Generic;
using System.Linq;

namespace PokerBot
{
    public struct Hand : IComparable<Hand>
    {
        public ulong Full;
        public long RanksOnly;
        public int Solo;
        public int DuplicatesMask;
        public int CardCount;    

    }
    
    class Program
    {
        //hearts = 0, spades = 1, diamonds = 2, clubs = 3
        public const long Hearts = 143163392;
        public const long Spades = 286326784;
        public const long Diamonds = 572653568;
        public const long Clubs = 1145307136;
        public const ulong Mask = 0b_111111111111111111111111111111;
        public static int Compare(ulong full, long ranksOnly, int solo, ulong full2,long ranksOnly2, int solo2)
        {
            int score1 = GetScore(full, ranksOnly, solo);
            int score2 = GetScore(full2, ranksOnly2, solo2);
            if (score1.CompareTo(score2) != 0){
                return score1.CompareTo(score2);
            }
            else
            {
                return GetTieBreaker(ranksOnly).CompareTo(GetTieBreaker(ranksOnly2 >> 1));
            }
        }
        public static bool isStraight(int solo)
        {
            int lsb = solo &= -solo;
            int normalized = solo / lsb;
            return normalized == 31;

        }
        
        public static int GetScore(ulong full, long ranksOnly, int solo)
        {
            if (solo == 16444)
            {
                return 5;
            }
            switch ((ranksOnly) % 15)
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
            bool flush = (full & 143163392) != 0 || (full & 286326784) != 0 || (full & 572653568) != 0 || (full & 1145307136) != 0;
            bool straight = isStraight(solo);
            if (isStraight(solo) && flush)
            {
                return 9;
            }
            else if (straight)
            {
                return 5;
            }
            else if (flush){
                if (solo == 31744)
                {
                    return 10;
                }
                return 6;
            }
            return 0;
        }
        public static long GetTieBreaker(long ranksOnly)
        {   /*int one = GetHighestRank(full);
            int two = GetHighestRank(full ^ (1 << one));
            int three = GetHighestRank(full ^ (1 << two));
            int four = GetHighestRank(full ^ (1 << three));
            int five = GetHighestRank(full ^ (1 << four));
            long tiebreaker = one << 16 | two << 12 | three << 8 | four << 4 | five << 0;
            return tiebreaker;*/
            int first = GetHighestRank(ranksOnly);
            int tiebreaker = first << 16;
            for (int i = 0; i < 4; i++)
            {
                first = GetHighestRank(ranksOnly ^ (1 << first));
                tiebreaker |= first << 16 - ((i+1) * 4) ;

            }
            return tiebreaker;
        }


        public static int GetHighestRank(long ranksOnly)
        {
            if (ranksOnly == 0)
                return 0;

            int pos = 0;
            ranksOnly = ranksOnly / 2;

            while (ranksOnly != 0)
            {
                ranksOnly = ranksOnly / 2;
                pos++;
            }

            return pos;
        }
        public static int GetHighestRank(ulong ranksOnly)
        {
            if (ranksOnly == 0)
                return 0;

            int pos = 0;
            ranksOnly = ranksOnly / 2;

            while (ranksOnly != 0)
            {
                ranksOnly = ranksOnly / 2;
                pos++;
            }

            return pos;
        }
        public static List<long> UnavailableHands(Hand myCards, Hand table, Hand villainCards)
        {
            long ranksOnly = myCards.RanksOnly;
            ulong suits = (myCards.Full ^ (ulong)ranksOnly); 
            List<long> unavailable = new List<long>();
            for (int i = 0; i < myCards.CardCount; i++)
            {
                int cardNum = 0;
                for (int j =myCards.CardCount;j >0;j++)
                {
                    if ((suits & (ulong)(1L << (15 + (4 * i) + j))) != 0)
                    {
                        cardNum = 1 << (15 + (4 * i) + j);
                    }
                   unavailable.Add((long)((ulong)(1L << GetHighestRank(ranksOnly)) | (1UL << cardNum)));
                   ranksOnly ^= GetHighestRank(ranksOnly);
                }
                



            }
            

        }
        public static void ParseAsBits(Dictionary<int, int> cards, out long ranksOnly, out ulong full, out int solo, out int duplicatesMask, out int cardCount)
        {
            solo = 0;
            cardCount = cards.Count();
            int[] ranks = cards.Keys.ToArray();
            Array.Sort(ranks);
            Array.Reverse(ranks);
            for (int i = 0; i < ranks.Length; i++)
            {
                solo |= 1 << ranks[i];
            }
            Dictionary<int, int> instances = new Dictionary<int, int>();
            ranksOnly = 0;
            duplicatesMask = 0;
            for (int i = 0; i <= cards.Count; i++)
            {
                duplicatesMask |= 1 << i;
            }
            for (int i = 0; i < ranks.Length; i++)
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
                        duplicatesMask = duplicatesMask ^ (1 << i);
                        
                    }
                    offset = instances[rank];
                }
                long addition = (long)Math.Pow(2, rank * 4);
                addition = addition << offset;
                ranksOnly |= addition;
            }
            ranksOnly = ranksOnly >> 1;
            full = 0;
            for (int i = 0; i < ranks.Length; i++)
            {
                full |= (1UL << (15 + (4 * i) + cards[ranks[i]]));
            }
            full |= (ulong)ranksOnly;
            

            

        }
        static void Main(string[] args)
        {
            Dictionary<int, int> myCardsDict = new Dictionary<int, int>();
            Dictionary<int, int> tableDict = new Dictionary<int, int>();
            long myCardsRanksOnly;
            ulong myCardsFull;
            int myCardsSolo;
            long tableRanksOnly;
            ulong tableFull;
            int tableSolo;
            ParseAsBits(myCardsDict, out myCardsRanksOnly, out myCardsFull, out myCardsSolo);
            ParseAsBits(tableDict, out tableRanksOnly, out tableFull, out tableSolo);
            Hand myHand = new Hand { Full = myCardsFull, RanksOnly = myCardsRanksOnly, Solo = myCardsSolo };
            Hand tableHand = new Hand { Full = tableFull, RanksOnly = tableRanksOnly, Solo = tableSolo };




        }
    }
}
