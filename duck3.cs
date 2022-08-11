using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PokerBot
{
    public static class Table
    {
        public static List<KeyValuePair<int, int>> Deck = new List<KeyValuePair<int, int>>();
        public static List<KeyValuePair<int, int>> HoleCards = new List<KeyValuePair<int, int>>();
        public static List<KeyValuePair<int, int>> CommunityCards = new List<KeyValuePair<int, int>>();
        public static void GetDeck(List<KeyValuePair<int,int>> holeCards, List<KeyValuePair<int,int>> communityCards)
        {
            HoleCards = holeCards;
            CommunityCards = communityCards;
            for (int i = 2; i <= 14; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    KeyValuePair<int, int> kvp = new KeyValuePair<int, int>(i, j);
                    if (!Deck.Contains(kvp) && !holeCards.Contains(kvp) && !communityCards.Contains(kvp))
                    {
                        Deck.Add(kvp);
                    }


                }
            }
        }
    }
    public static class Poker
    {
        private static Dictionary<long, int> _tieTable = new Dictionary<long, int>();
        private static Dictionary<List<KeyValuePair<int,int>>,Hand> _handTable = new Dictionary<List<KeyValuePair<int,int>>,Hand>();


        private static int CompareKVP(KeyValuePair<int, int> a, KeyValuePair<int, int> b)
        {
            return a.Key.CompareTo(b.Key);
        }



        private static IEnumerable<List<KeyValuePair<int, int>>> cardCombos(IEnumerable<KeyValuePair<int, int>> cards, int count)
        {
            int i = 0;
            foreach (var card in cards)
            {
                if (count == 1)
                {
                    yield return new List<KeyValuePair<int, int>>() { card };
                }

                else
                {
                    foreach (var result in cardCombos(cards.Skip(i + 1), count - 1))
                    {

                        yield return new List<KeyValuePair<int, int>>(result) { card };
                    }
                    //yield return new Card[] { card }.Concat(result);
                }

                ++i;
            }
        }

        private static Hand handToPlay(List<KeyValuePair<int, int>> holeCards, List<KeyValuePair<int, int>> tableCards)
        {


            Hand maxHand = new Hand { Score = -1000, TieBreaker = -1000 };
            List<List<KeyValuePair<int, int>>> allCombos = new List<List<KeyValuePair<int, int>>>(cardCombos(tableCards, 3));
            allCombos.AddRange(cardCombos(tableCards, 4));
            foreach (var combosList in allCombos)
            {

                if (combosList.Count == 3)
                {
                    List<KeyValuePair<int, int>> myHand = new List<KeyValuePair<int, int>>(combosList);
                    myHand.AddRange(holeCards);
                    Hand hand = ParseAsHand(myHand);
                    if (hand > maxHand)
                    {
                        maxHand = hand;
                    }
                }
                else
                {
                    for (int i = 0; i < 2; i++)
                    {
                        List<KeyValuePair<int, int>> myHand = new List<KeyValuePair<int, int>>(combosList);
                        myHand.Add(holeCards[i]);
                        Hand hand = ParseAsHand(myHand);
                        if (hand > maxHand)
                        {
                            maxHand = hand;
                        }
                    }
                }


            }
            return maxHand;
        }
        private static (float heroPoints, float villainPoints) updateScore(List<KeyValuePair<int, int>> myHoleCards, List<KeyValuePair<int, int>> villainHoleCards, List<KeyValuePair<int, int>> tableCards)
        {
            Hand heroHand = handToPlay(myHoleCards, tableCards);
            Hand villainHand = handToPlay(villainHoleCards, tableCards);
            switch (heroHand.CompareTo(villainHand))
            {
                case 1:
                    return (1, 0);
                case -1:
                    return (0, 1);
                default:
                    return (0.5f, 0.5f);
            }

        }
       
        private static long getTieBreaker(long allCards)
        {
            if (_tieTable.ContainsKey(allCards))
            {
                return _tieTable[allCards];
            }
            int first = getHighestRank(allCards);
            int tiebreaker = first << 16;
            for (int i = 0; i < 4; i++)
            {
                first = getHighestRank(allCards ^ (1 << first));
                tiebreaker |= first << 16 - ((i + 1) * 4);

            }
            _tieTable[allCards] = tiebreaker;
            return tiebreaker;
        }

        private static bool isStraight(long solo)
        {
            long lsb = solo &= -solo;
            long normalized = solo / lsb;
            return normalized == 31;

        }
        /*private static int getHighestRank(long allCards)
        {
            if (HighRankTable.ContainsKey(allCards))
            {
                Console.WriteLine("ok");
                return HighRankTable[allCards];
            }
            if (allCards == 0)
                return 0;

            int pos = 0;
            allCards = allCards / 2;

            while (allCards != 0)
            {
                allCards = allCards / 2;
                pos++;
            }
            //HighRankTable[allCards] = pos;
            return pos;
        }*/
        private static int getHighestRank(long allCards)
        {
            return 63 - BitOperations.LeadingZeroCount((ulong)allCards | 1);
        }
   
        private static int getScore(long solo, long allCards, long suits)
        {
            bool flush = suits == 4369 || suits == 8738 || suits == 17476 || suits == 34952;
            bool straight = isStraight(solo);
            if (straight && flush)
            {
                if (solo == 31744)
                {
                    return 10;
                }
                return 9;
            }
            int score = 0;
            switch ((allCards) % 15)
            {
                case 1:
                    return 8;
                case 10:
                    return 7;
                case 9:
                    score = 4;
                    break;
                case 7:
                    score = 3;
                    break;
                case 6:
                    score = 2;
                    break;
                case 5:
                    score = 1;
                    break;
                default:
                    break;
            }
            if (flush)
            {
                return 6;
            }
            else if (straight || solo == 16444)
            {

                return 5;
            }
            return score;
        }
        public static Hand ParseAsHand(List<KeyValuePair<int, int>> cards)
        {



            cards.Sort(CompareKVP);
            cards.Reverse();
            /*if (_handTable.TryGetValue(cards, out Hand valueHand))
            {
                Console.WriteLine("found");
                return valueHand;
            }*/
            long solo = 0;
            int[] ranks = cards.Select(kvp => kvp.Key).ToArray();
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
                long addition = 1 << (rank << 2);
                addition = addition << offset;
                allCards |= addition;
            }
            allCards = allCards >> 1;

            long suits = 0;
            for (int i = 0; i < cards.Count; i++)
            {
                suits |= (1L << ((4 * i) + cards[i].Value));
            }
           return new Hand(){ Score = getScore(solo, allCards, suits), TieBreaker = getTieBreaker(allCards) };
            /*_handTable.Add(cards, hand);
            return hand;*/
        }
        public static float GetOdds(List<KeyValuePair<int, int>> holeCardsKvpList, List<KeyValuePair<int, int>> tableKvpList)
        {
            float heroWins = 0; //if player wins
            float villainWins = 0; //if opponent wins
            float villainCardSets = 0;
            Console.WriteLine(holeCardsKvpList.Count);
            Console.WriteLine(tableKvpList.Count);
            foreach (var villainCardSet in cardCombos(Table.Deck, 2)) //every combination of cards our opponent may have
            {
                villainCardSets++;
                Console.WriteLine(villainCardSets);
                List<List<KeyValuePair<int, int>>> validHands = new List<List<KeyValuePair<int, int>>>(); //all 5-card sets that may potentially appear on the table
                foreach (var cardSet in cardCombos(Table.Deck.Except(villainCardSet), 5 - tableKvpList.Count)) //all combinations of cards that may be added to the existing table
                {
                    List<KeyValuePair<int, int>> currentHand = new List<KeyValuePair<int, int>>(tableKvpList);
                    foreach (var card in cardSet)
                    {
                        currentHand.Add(card); //add the combination to the 5-card set
                    }
                    (float heroPoints, float villainPoints) scoreUpdate = updateScore(holeCardsKvpList, villainCardSet, currentHand);
                    heroWins += scoreUpdate.heroPoints;
                    villainWins += scoreUpdate.villainPoints;

                }

            }
            return heroWins / (heroWins + villainWins);

        }
    }


    public struct Hand : IComparable<Hand>
    {

        public int Score;
        public long TieBreaker;
        public int CompareTo(Hand hand)
        {
            int scoreComparer = Score.CompareTo(hand.Score);
            if (scoreComparer != 0)
            {
                return scoreComparer;
            }
            else
            {
                return TieBreaker.CompareTo(hand.TieBreaker);
            }
        }

        public static bool operator >(Hand hand1, Hand hand2)
        {
            return hand1.CompareTo(hand2) > 0;
        }
        public static bool operator <(Hand hand1, Hand hand2)
        {
            return hand1.CompareTo(hand2) < 0;
        }



    }

    class Program
    {

      

       
      

      
       

       


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

        
       
        
       



        static void Main(string[] args)
        {

          
            int[] holeCardSuits = new int[] { 0, 0 };
            int[] holeCardRanks = new int[] { 14, 13 };
            int[] tableCardSuits = new int[] { 2, 3, 1 };
            int[] tableCardRanks = new int[] { 10, 8, 4 };
            List<KeyValuePair<int, int>> myHoleCards = new List<KeyValuePair<int, int>>();
            List<KeyValuePair<int, int>> tableCards = new List<KeyValuePair<int, int>>();

            for (int i = 0; i < 2; i++)
            {
                myHoleCards.Add(new KeyValuePair<int, int>(holeCardRanks[i], holeCardSuits[i]));
            }
            Console.WriteLine(myHoleCards.Count);

            for (int i = 0; i < 3; i++)
            {
                tableCards.Add(new KeyValuePair<int, int>(tableCardRanks[i], tableCardSuits[i]));
            }
            Table.GetDeck(myHoleCards,tableCards);
            Console.WriteLine(tableCards.Count);
            float odds = Poker.GetOdds(myHoleCards, tableCards);
            Console.WriteLine(odds);


        }
    }
}
