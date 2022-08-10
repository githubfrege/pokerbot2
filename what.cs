using System;
using System.Collections.Generic;
using System.Linq;

namespace PokerBot
{
    public static class HandOperations
    {

    }
    public struct WinCounter
    {
        public int HeroWins;
        public int VillainWins;
        public int TotalWins;
    }

    public struct Hand : IComparable<Hand>
    {

        public int Score;
        public long TieBreaker;
        public  int CompareTo(Hand hand)
        {
            int scoreComparer = Score.CompareTo(hand.Score);
            if ( scoreComparer != 0)
            {
                return scoreComparer;
            }
            else
            {
                return TieBreaker.CompareTo(hand.TieBreaker);
            }
        }
        
        
        


    }
    
    class Program
    {

        public static List<KeyValuePair<int,int>> Deck = new  List<KeyValuePair<int, int>>();
        public static List<KeyValuePair<int, int>> myCardsKvpList = new List<KeyValuePair<int, int>>();
        public static List<KeyValuePair<int, int>> tableKvpList = new List<KeyValuePair<int, int>>();

        public static bool isStraight(long solo)
        {
            long lsb = solo &= -solo;
            long normalized = solo / lsb;
            return normalized == 31;

        }
        public static Hand ParseAsHand(List<KeyValuePair<int, int>> cards)
        {
            long solo = 0;
            int cardCount = cards.Count();
            int[] ranks = cards.Select(kvp => kvp.Key).ToArray();
            Array.Sort(ranks);
            Array.Reverse(ranks);
            for (int i = 0; i < ranks.Length; i++)
            {
                solo |= 1L << ranks[i];
            }
            Dictionary<int, int> instances = new Dictionary<int, int>();
            long allCards = 0;

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

                    }
                    offset = instances[rank];
                }
                long addition = (long)Math.Pow(2, rank * 4);
                addition = addition << offset;
                allCards |= addition;
            }
            allCards = allCards >> 1;
            Array.Reverse(ranks);
            long suits = 0;
            for (int i = 0; i < cards.Count; i++)
            {
                suits |= (1L << ((4 * i) + cards[ranks[i]].Value));
            }
            return new Hand { Score = GetScore(solo, allCards, suits), TieBreaker = GetTieBreaker(allCards) };
        }

        public static int GetScore(long solo, long allCards, long suits)
        {
            bool flush = suits == 4369 || suits == 8738 || suits == 17476 || suits == 34952;
            bool straight = isStraight(solo);
            switch ((allCards) % 15)
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
         
            if (straight && flush)
            {
                if (solo == 31744)
                {
                    return 10;
                }
                return 9;
            }
            else if (flush)
            {
                return 6;
            }
            else if (straight || solo == 16444) 
            {
               
                return 6;
            }
            return 0;
        }
        public static long GetTieBreaker(long allCards)
        {
            int first = GetHighestRank(allCards);
            int tiebreaker = first << 16;
            for (int i = 0; i < 4; i++)
            {
                first = GetHighestRank(allCards ^ (1 << first));
                tiebreaker |= first << 16 - ((i + 1) * 4);

            }
            return tiebreaker;
        }


        public static int GetHighestRank(long allCards)
        {
            if (allCards == 0)
                return 0;

            int pos = 0;
            allCards = allCards / 2;

            while (allCards != 0)
            {
                allCards = allCards / 2;
                pos++;
            }

            return pos;
        }


        /*public static List<ulong> GetHandList(Hand hand)
        {
            long ranksOnly = hand.Card;
            ulong suits = (hand.Full ^ (ulong)ranksOnly);
            List<ulong> handList = new List<ulong>();
            for (int i = hand.CardCount; i > 0; i++)
            {
                int cardNum = 0;
                for (int j = 4; j > 0; j++)
                {
                    if ((suits & (ulong)(1L << (15 + (4 * i) + j))) != 0)
                    {
                        cardNum = 1 << (15 + (4 * i) + j);
                    }
                    handList.Add(((ulong)(1L << GetHighestRank(ranksOnly)) | (1UL << cardNum)));
                    ranksOnly ^= GetHighestRank(ranksOnly);
                }
            }
            return handList;
        }*/

        public static void GetDeck()
        {
            for (int i = 2; i <= 14; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Deck.Add(new KeyValuePair<int, int>(i, j));
                    
                }
            }
        }
        public static (int heroPoints, int villainPoints) UpdateScore(ref WinCounter wc,List<KeyValuePair<int,int>> myHoleCards, List<KeyValuePair<int, int>> villainHoleCards, List<KeyValuePair<int, int>> tableCards )
        {
            (int, int) scoreHandler = (0, 0);
            Hand heroHand = new Hand { Score = -1000, TieBreaker = -1000 };
            Hand villainHand = new Hand { Score = -1000, TieBreaker = -1000 }; ;
            for (int i = 0;i< 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    List<KeyValuePair<int, int>> myHand = new List<KeyValuePair<int, int>>(tableCards.GetRange(i,4));
                    myHand.Add(myHoleCards[j]);
                    Hand hand = ParseAsHand(myHand);
                    if (hand.Score > max)
                    {

                    }
                }
                
            }
        }
        public static void GetOdds()
        {
            float heroWins = 0; //if player wins
            float villainWins = 0; //if opponent wins
            float villainCardSets = 0;
            WinCounter winCounter = new WinCounter();
            List<KeyValuePair<int,int>> availCards = (Deck.Except(myCardsKvpList).Except(tableKvpList)).ToList(); //all cards left in the deck (original deck with my hole cards and community cards subtracted)
            foreach (var villainCardSet in CardCombos(availCards, 2)) //every combination of cards our opponent may have
            {
                Console.WriteLine($"vcset n {villainCardSets} ");
                villainCardSets++;
                List<List<KeyValuePair<int,int>>> validHands = new List<List<KeyValuePair<int,int>>>(); //all 5-card sets that may potentially appear on the table
                foreach (var cardSet in CardCombos(availCards.Except(villainCardSet), 5 - tableKvpList.Count)) //all combinations of cards that may be added to the existing table
                {
                    List<KeyValuePair<int,int>> currentHand = new List<KeyValuePair<int,int>>(tableKvpList);
                    foreach (var card in cardSet)
                    {
                        currentHand.Add(card); //add the combination to the 5-card set
                    }
                    UpdateScore(ref winCounter, myCardsKvpList, villainCardSet, currentHand);


                }

                /*foreach (List<ulong> hand in validHands)
                {
                    Hand villainHand = HandToPlay(villainCardSet, hand);
                    Hand heroHand = HandToPlay(MyCards, hand);
                    switch (heroHand.CompareTo(villainHand))
                    {
                        case 1:
                            heroWins++;
                            break;
                        case -1:
                            villainWins++;
                            break;
                        case 0:
                            heroWins += 0.5f;
                            villainWins += 0.5f;
                            break;
                    }

                }*/

            }
            Odds = heroWins / (heroWins + villainWins);

        }

     
        public static IEnumerable<List<KeyValuePair<int,int>>> CardCombos(IEnumerable<KeyValuePair<int,int>> cards, int count)
        {
            int i = 0;
            foreach (var card in cards)
            {
                if (count == 1)
                {
                    yield return new List<KeyValuePair<int,int>>() { card };
                }

                else
                {
                    foreach (var result in CardCombos(cards.Skip(i + 1), count - 1))
                    {

                        yield return new List<KeyValuePair<int,int>>(result) { card };
                    }
                    //yield return new Card[] { card }.Concat(result);
                }

                ++i;
            }
        }

      
        static void Main(string[] args)
        {
            
           



        }
    }
}
