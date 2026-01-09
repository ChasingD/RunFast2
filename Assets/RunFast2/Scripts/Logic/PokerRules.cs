using System;
using System.Collections.Generic;
using System.Linq;
using RunFast2.Scripts.Model;
using UnityEngine;

namespace RunFast2.Scripts.Logic
{
    public static class PokerRules
    {
        public static PokerHand AnalyzeHand(List<Card> cards, bool threeAsBomb = false)
        {
            if (cards == null || cards.Count == 0) return new PokerHand(HandType.Invalid, 0, cards ?? new List<Card>());

            cards.Sort((a, b) => a.GetLogicWeight().CompareTo(b.GetLogicWeight()));

            int count = cards.Count;
            int firstWeight = cards[0].GetLogicWeight();
            int lastWeight = cards[count - 1].GetLogicWeight();

            // --- Single ---
            if (count == 1)
            {
                return new PokerHand(HandType.Single, firstWeight, cards);
            }

            // --- Pair ---
            if (count == 2)
            {
                if (firstWeight == lastWeight)
                    return new PokerHand(HandType.Pair, firstWeight, cards);
            }

            // --- Triplet (3) ---
            if (count == 3)
            {
                if (cards[0].GetLogicWeight() == cards[2].GetLogicWeight())
                {
                    // AAA Bomb Rule
                    if (threeAsBomb && cards[0].Rank == CardRank.Ace)
                    {
                        // AAA is the largest Bomb. Give it very high weight.
                        return new PokerHand(HandType.Bomb, 999, cards);
                    }

                    return new PokerHand(HandType.Triplet, firstWeight, cards);
                }
            }

            // --- Bomb (4) ---
            if (count == 4)
            {
                if (cards[0].GetLogicWeight() == cards[3].GetLogicWeight())
                    return new PokerHand(HandType.Bomb, firstWeight, cards);
            }

            // Pre-calculate groups
            var groups = cards.GroupBy(c => c.GetLogicWeight())
                              .Select(g => new { Weight = g.Key, Count = g.Count() })
                              .OrderByDescending(g => g.Count)
                              .ThenByDescending(g => g.Weight)
                              .ToList();

            // --- Triplet with Two (3+2) ---
            if (count == 5)
            {
                // 3 + 2 (Triplet + Random wings)
                // The triplet is the dominant part
                if (groups[0].Count == 3)
                {
                     return new PokerHand(HandType.TripletWithTwo, groups[0].Weight, cards);
                }

                if (IsStraight(cards))
                {
                    return new PokerHand(HandType.Straight, lastWeight, cards);
                }
            }

            // --- Consecutive Pairs ---
            if (count >= 4 && count % 2 == 0)
            {
                if (IsConsecutivePairs(cards))
                {
                    // Weight is the rank of the largest pair
                    return new PokerHand(HandType.ConsecutivePairs, cards[count-1].GetLogicWeight(), cards);
                }
            }

            // --- Straight ---
            if (count >= 5)
            {
                if (IsStraight(cards))
                    return new PokerHand(HandType.Straight, lastWeight, cards);
            }

            // --- Airplane (N * 5 cards) ---
            if (count % 5 == 0)
            {
                int n = count / 5;
                // Find n consecutive triplets
                var triplets = groups.Where(g => g.Count >= 3).OrderBy(g => g.Weight).ToList();

                // Sliding window to find consecutive triplets
                for (int i = 0; i <= triplets.Count - n; i++)
                {
                    var window = triplets.Skip(i).Take(n).Select(t => t.Weight).ToList();

                    if (IsConsecutive(window))
                    {
                        // Ensure no 2s in the body of airplane
                        if (window.Any(w => w >= (int)CardRank.Two)) continue;

                        int maxWeight = window.Last();
                        return new PokerHand(HandType.Airplane, maxWeight, cards);
                    }
                }
            }

            // --- 4 with 3 ---
            if (count == 7)
            {
                 if (groups[0].Count == 4)
                 {
                     return new PokerHand(HandType.FourWithThree, groups[0].Weight, cards);
                 }
            }

            return new PokerHand(HandType.Invalid, 0, cards);
        }

        public static bool CanBeat(PokerHand prev, PokerHand curr)
        {
            if (curr.Type == HandType.Invalid) return false;

            if (curr.Type == HandType.Bomb)
            {
                if (prev.Type != HandType.Bomb) return true;
                // AAA (weight 999) will beat standard bombs (weight <= 15)
                return curr.Weight > prev.Weight;
            }
            if (prev.Type == HandType.Bomb) return false;

            if (curr.Type != prev.Type) return false;
            if (curr.Cards.Count != prev.Cards.Count) return false;

            return curr.Weight > prev.Weight;
        }

        private static bool IsStraight(List<Card> cards)
        {
            // No 2s
            if (cards.Any(c => c.Rank == CardRank.Two)) return false;

            for (int i = 0; i < cards.Count - 1; i++)
            {
                if (cards[i + 1].GetLogicWeight() != cards[i].GetLogicWeight() + 1)
                    return false;
            }
            return true;
        }

        private static bool IsConsecutivePairs(List<Card> cards)
        {
            // No 2s
            if (cards.Any(c => c.Rank == CardRank.Two)) return false;

            int pairs = cards.Count / 2;
            for (int i = 0; i < pairs; i++)
            {
                // Verify Pair
                if (cards[2 * i].GetLogicWeight() != cards[2 * i + 1].GetLogicWeight())
                    return false;

                // Verify Sequence
                if (i < pairs - 1)
                {
                    if (cards[2 * (i + 1)].GetLogicWeight() != cards[2 * i].GetLogicWeight() + 1)
                        return false;
                }
            }
            return true;
        }

        private static bool IsConsecutive(List<int> weights)
        {
            if (weights.Count < 2) return true;

            for (int i = 0; i < weights.Count - 1; i++)
            {
                if (weights[i + 1] != weights[i] + 1) return false;
            }
            return true;
        }
    }
}
