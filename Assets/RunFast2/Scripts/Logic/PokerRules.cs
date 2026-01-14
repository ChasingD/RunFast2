using System;
using System.Collections.Generic;
using System.Linq;
using RunFast2.Scripts.Model;
using UnityEngine;

namespace RunFast2.Scripts.Logic
{
    public static class PokerRules
    {
        private struct CardGroupData
        {
            public int Weight;
            public int Count;
        }
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
        /// <summary>
        /// 检查手牌中是否有能管住 targetHand 的组合
        /// </summary>
        public static bool HasHandToBeat(List<Card> myCards, PokerHand targetHand)
        {
            if (myCards == null || myCards.Count == 0) return false;

            // 1. 如果上家没出牌（或者是我自己出的），那我肯定能出（只要有牌）
            if (targetHand == null || targetHand.Type == HandType.Invalid) return true;

            // 2. 先整理手牌：分组统计 (Key=Weight, Value=Count)
            List<CardGroupData> groups = myCards.GroupBy(c => c.GetLogicWeight())
                .Select(g => new CardGroupData { Weight = g.Key, Count = g.Count() })
                .OrderBy(g => g.Weight)
                .ToList();

            // 3. 检查是否有炸弹 (炸弹能管住非炸弹，大炸弹管住小炸弹)
            // 跑得快通常是4张算炸，也有3A算炸的规则，这里按标准4张处理
            var myBombs = groups.Where(g => g.Count == 4).ToList();
            
            // 如果上家不是炸弹，只要我有炸弹就能管
            if (targetHand.Type != HandType.Bomb && myBombs.Count > 0) return true;
            
            // 如果上家是炸弹，我需要比他大的炸弹
            if (targetHand.Type == HandType.Bomb)
            {
                if (myBombs.Any(b => b.Weight > targetHand.Weight)) return true;
                // 如果没有更大的炸弹，且又是炸弹对炸弹，那后面同牌型判断也就没戏了，直接返回 false
                return false; 
            }

            // 4. 同牌型判断 (非炸弹情况)
            switch (targetHand.Type)
            {
                case HandType.Single:
                    // 找一张比目标大的单牌
                    return groups.Any(g => g.Weight > targetHand.Weight);

                case HandType.Pair:
                    // 找一对且比目标大
                    return groups.Any(g => g.Count >= 2 && g.Weight > targetHand.Weight);

                case HandType.Triplet:
                case HandType.TripletWithTwo: // 跑得快三带二通常只比三张的大小
                    // 找三张且比目标大
                    return groups.Any(g => g.Count >= 3 && g.Weight > targetHand.Weight);

                case HandType.Straight:
                    // 顺子比较复杂：需要找 长度相同 且 尾牌更大 的顺子
                    // 简单做法：遍历所有可能的起始点
                    return HasStraightToBeat(groups, targetHand.Cards.Count, targetHand.Weight);

                case HandType.ConsecutivePairs: // 连对
                    return HasConsecutivePairsToBeat(groups, targetHand.Cards.Count / 2, targetHand.Weight);

                // ... 其他牌型(飞机等)如果需要完美支持，需继续添加逻辑
                // 现在的逻辑至少覆盖了 单、对、三、顺、炸弹 这 90% 的情况
            }

            return false;
        }

        /// <summary>
        /// 获取能管住 targetHand 的最小牌型组合
        /// </summary>
        public static List<Card> GetSmallestBeatHand(List<Card> myCards, PokerHand targetHand)
        {
            if (myCards == null || myCards.Count == 0) return null;

            // 1. 整理手牌
            List<CardGroupData> groups = myCards.GroupBy(c => c.GetLogicWeight())
                .Select(g => new CardGroupData { Weight = g.Key, Count = g.Count() })
                .OrderBy(g => g.Weight)
                .ToList();

            // 2. 查找同牌型
            if (targetHand.Type != HandType.Bomb)
            {
                switch (targetHand.Type)
                {
                    case HandType.Single:
                        var single = groups.FirstOrDefault(g => g.Weight > targetHand.Weight);
                        if (single.Count > 0) return GetCardsByWeight(myCards, single.Weight, 1);
                        break;

                    case HandType.Pair:
                        var pair = groups.FirstOrDefault(g => g.Count >= 2 && g.Weight > targetHand.Weight);
                        if (pair.Count > 0) return GetCardsByWeight(myCards, pair.Weight, 2);
                        break;

                    case HandType.Triplet:
                    case HandType.TripletWithTwo:
                        var triplet = groups.FirstOrDefault(g => g.Count >= 3 && g.Weight > targetHand.Weight);
                        if (triplet.Count > 0)
                        {
                            var result = GetCardsByWeight(myCards, triplet.Weight, 3);
                            // 如果是三带二，还需要找两张散牌
                            if (targetHand.Type == HandType.TripletWithTwo)
                            {
                                var wings = GetSmallestWings(myCards, result, 2);
                                if (wings != null) result.AddRange(wings);
                            }
                            return result;
                        }
                        break;

                    case HandType.Straight:
                        var straight = FindSmallestStraight(groups, targetHand.Cards.Count, targetHand.Weight);
                        if (straight != null) return GetCardsFromWeights(myCards, straight);
                        break;

                    case HandType.ConsecutivePairs:
                        var pairs = FindSmallestConsecutivePairs(groups, targetHand.Cards.Count / 2, targetHand.Weight);
                        if (pairs != null) return GetCardsFromWeights(myCards, pairs, 2);
                        break;
                }
            }

            // 3. 查找炸弹
            var myBombs = groups.Where(g => g.Count == 4).OrderBy(g => g.Weight).ToList();
            if (targetHand.Type != HandType.Bomb)
            {
                if (myBombs.Count > 0) return GetCardsByWeight(myCards, myBombs[0].Weight, 4);
            }
            else
            {
                var biggerBomb = myBombs.FirstOrDefault(b => b.Weight > targetHand.Weight);
                if (biggerBomb.Count > 0) return GetCardsByWeight(myCards, biggerBomb.Weight, 4);
            }

            return null;
        }

        // --- 辅助方法 ---

        private static List<Card> GetCardsByWeight(List<Card> source, int weight, int count)
        {
            return source.Where(c => c.GetLogicWeight() == weight).Take(count).ToList();
        }

        private static List<Card> GetCardsFromWeights(List<Card> source, List<int> weights, int countPerWeight = 1)
        {
            List<Card> result = new List<Card>();
            foreach (var w in weights)
            {
                result.AddRange(GetCardsByWeight(source, w, countPerWeight));
            }
            return result;
        }

        private static List<Card> GetSmallestWings(List<Card> source, List<Card> exclude, int count)
        {
            var available = source.Except(exclude).OrderBy(c => c.GetLogicWeight()).ToList();
            if (available.Count < count) return null;
            return available.Take(count).ToList();
        }

        private static List<int> FindSmallestStraight(List<CardGroupData> groups, int length, int targetMaxWeight)
        {
            var validWeights = groups.Where(g => g.Weight < 15).Select(g => g.Weight).OrderBy(w => w).ToList();
            if (validWeights.Count < length) return null;

            for (int i = 0; i <= validWeights.Count - length; i++)
            {
                bool isSeq = true;
                for (int j = 0; j < length - 1; j++)
                {
                    if (validWeights[i + j + 1] != validWeights[i + j] + 1)
                    {
                        isSeq = false;
                        break;
                    }
                }

                if (isSeq)
                {
                    int myMaxWeight = validWeights[i + length - 1];
                    if (myMaxWeight > targetMaxWeight)
                    {
                        return validWeights.Skip(i).Take(length).ToList();
                    }
                }
            }
            return null;
        }

        private static List<int> FindSmallestConsecutivePairs(List<CardGroupData> groups, int pairCount, int targetMaxWeight)
        {
            var pairs = groups.Where(g => g.Count >= 2 && g.Weight < 15).Select(g => g.Weight).OrderBy(w => w).ToList();
            if (pairs.Count < pairCount) return null;

            for (int i = 0; i <= pairs.Count - pairCount; i++)
            {
                bool isSeq = true;
                for (int j = 0; j < pairCount - 1; j++)
                {
                    if (pairs[i + j + 1] != pairs[i + j] + 1)
                    {
                        isSeq = false;
                        break;
                    }
                }
                if (isSeq)
                {
                    if (pairs[i + pairCount - 1] > targetMaxWeight)
                    {
                        return pairs.Skip(i).Take(pairCount).ToList();
                    }
                }
            }
            return null;
        }

        private static bool HasStraightToBeat(IEnumerable<CardGroupData> groups, int length, int targetMaxWeight)
        {
            // 提取所有不含2的牌 (跑得快规则：2不能当顺子)
            // 注意：GetLogicWeight() 中，2通常是15，A是14，K是13... 
            // 顺子最大通常到A。如果你的逻辑里2的权重很高，要排除掉
            var validWeights = groups.Where(g => g.Weight < 15) // 假设15是2
                                     .Select(g => g.Weight)
                                     .OrderBy(w => w)
                                     .ToList();

            if (validWeights.Count < length) return false;

            // 滑动窗口查找
            for (int i = 0; i <= validWeights.Count - length; i++)
            {
                // 检查窗口内的牌是否连续
                bool isSeq = true;
                for (int j = 0; j < length - 1; j++)
                {
                    if (validWeights[i + j + 1] != validWeights[i + j] + 1)
                    {
                        isSeq = false;
                        break;
                    }
                }

                if (isSeq)
                {
                    int myMaxWeight = validWeights[i + length - 1];
                    if (myMaxWeight > targetMaxWeight) return true;
                }
            }
            return false;
        }

        private static bool HasConsecutivePairsToBeat(IEnumerable<CardGroupData> groups, int pairCount, int targetMaxWeight)
        {
            // 找对子
            var pairs = groups.Where(g => g.Count >= 2 && g.Weight < 15) // 排除2
                              .Select(g => g.Weight)
                              .OrderBy(w => w)
                              .ToList();

            if (pairs.Count < pairCount) return false;

            // 类似顺子的逻辑
            for (int i = 0; i <= pairs.Count - pairCount; i++)
            {
                bool isSeq = true;
                for (int j = 0; j < pairCount - 1; j++)
                {
                    if (pairs[i + j + 1] != pairs[i + j] + 1)
                    {
                        isSeq = false;
                        break;
                    }
                }
                if (isSeq)
                {
                    if (pairs[i + pairCount - 1] > targetMaxWeight) return true;
                }
            }
            return false;
        }
    }
}