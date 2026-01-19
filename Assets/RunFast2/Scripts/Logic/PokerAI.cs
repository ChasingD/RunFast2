using System.Collections.Generic;
using System.Linq;
using RunFast2.Scripts.Model;
using UnityEngine;

namespace RunFast2.Scripts.Logic
{
    public static class PokerAI
    {
        /// <summary>
        /// 获取最佳出牌策略
        /// </summary>
        /// <param name="myHand">我的手牌</param>
        /// <param name="lastHand">上家出的牌（如果是首发则为null）</param>
        /// <returns>要出的牌列表，如果不要则返回 null</returns>
        public static List<Card> GetBestMove(List<Card> myHand, PokerHand lastHand)
        {
            // 1. 如果是跟牌（必须管上）
            if (lastHand != null)
            {
                // 进阶跟牌策略：
                // 如果上家出的是单张，且我有大牌，可以考虑压死
                // 如果上家出的是小对子，我有小对子就跟，没有就拆
                // 目前保持简单：找最小能管上的牌，保留炸弹
                
                var beatHand = PokerRules.GetSmallestBeatHand(myHand, lastHand);
                
                // 如果能管上，且不是炸弹，直接出
                if (beatHand != null)
                {
                    var handType = PokerRules.AnalyzeHand(beatHand).Type;
                    if (handType != HandType.Bomb) return beatHand;
                    
                    // 如果是炸弹，且上家不是炸弹，考虑是否要炸
                    // 策略：如果我手牌少于5张，或者对方手牌少于5张，就炸
                    // 或者如果我只剩炸弹了，也炸
                    bool isLastHand = myHand.Count == beatHand.Count;
                    if (isLastHand) return beatHand;

                    // 简单起见，如果上家不是炸弹，且我有炸弹，有50%概率炸，或者手牌少时必炸
                    if (myHand.Count <= 5) return beatHand;
                    
                    // 否则保留炸弹，选择过牌 (返回 null)
                    return null;
                }
                
                return null;
            }

            // 2. 如果是首发（自由出牌）
            if (myHand.Count == 0) return null;

            // 进阶首发策略：
            // 优先出顺子、连对、三带一等复杂牌型，保留单张和对子作为垫底
            // 如果有炸弹，尽量留到最后或关键时刻

            // A. 尝试出顺子 (5张起)
            var straight = FindLongestStraight(myHand);
            if (straight != null) return straight;

            // B. 尝试出连对 (2连对起)
            var pairs = FindLongestPairStraight(myHand);
            if (pairs != null) return pairs;

            // C. 尝试出三带一 / 三带二
            var threeWith = FindThreeWith(myHand);
            if (threeWith != null) return threeWith;

            // D. 尝试出对子 (从小到大)
            var pair = FindSmallestPair(myHand);
            if (pair != null) return pair;

            // E. 最后出单张 (从小到大)
            var sortedHand = myHand.OrderBy(c => c.GetLogicWeight()).ToList();
            return new List<Card> { sortedHand[0] };
        }

        // --- 辅助查找方法 ---

        private static List<Card> FindLongestStraight(List<Card> hand)
        {
            // 简单实现：查找最长的顺子
            // 排除 2 和 A (在某些规则中 A 可以算顺子，这里假设 A, 2 不参与顺子)
            // 假设规则：3-A 可以连，2 不能连
            
            var validCards = hand.Where(c => c.Rank != CardRank.Two).OrderBy(c => c.GetLogicWeight()).ToList();
            if (validCards.Count < 5) return null;

            // 去重 Rank
            var distinctRanks = validCards.GroupBy(c => c.Rank).Select(g => g.First()).ToList();
            
            List<Card> bestStraight = null;
            
            for (int i = 0; i < distinctRanks.Count; i++)
            {
                List<Card> currentStraight = new List<Card> { distinctRanks[i] };
                for (int j = i + 1; j < distinctRanks.Count; j++)
                {
                    if (distinctRanks[j].GetLogicWeight() == distinctRanks[j - 1].GetLogicWeight() + 1)
                    {
                        currentStraight.Add(distinctRanks[j]);
                    }
                    else
                    {
                        break;
                    }
                }

                if (currentStraight.Count >= 5)
                {
                    // 贪婪：找最长的
                    if (bestStraight == null || currentStraight.Count > bestStraight.Count)
                    {
                        bestStraight = currentStraight;
                    }
                }
            }

            return bestStraight;
        }

        private static List<Card> FindLongestPairStraight(List<Card> hand)
        {
            // 查找连对
            var pairs = hand.GroupBy(c => c.Rank)
                            .Where(g => g.Count() >= 2 && g.Key != CardRank.Two) // 排除2
                            .OrderBy(g => g.First().GetLogicWeight())
                            .Select(g => g.Take(2).ToList())
                            .ToList();

            if (pairs.Count < 2) return null;

            List<Card> bestPairs = null;

            for (int i = 0; i < pairs.Count; i++)
            {
                List<Card> currentPairs = new List<Card>();
                currentPairs.AddRange(pairs[i]);

                for (int j = i + 1; j < pairs.Count; j++)
                {
                    if (pairs[j][0].GetLogicWeight() == pairs[j - 1][0].GetLogicWeight() + 1)
                    {
                        currentPairs.AddRange(pairs[j]);
                    }
                    else
                    {
                        break;
                    }
                }

                if (currentPairs.Count >= 4) // 至少2连对
                {
                    if (bestPairs == null || currentPairs.Count > bestPairs.Count)
                    {
                        bestPairs = currentPairs;
                    }
                }
            }

            return bestPairs;
        }

        private static List<Card> FindThreeWith(List<Card> hand)
        {
            // 查找三带一或三带二
            var threes = hand.GroupBy(c => c.Rank)
                             .Where(g => g.Count() >= 3)
                             .OrderBy(g => g.First().GetLogicWeight())
                             .FirstOrDefault();

            if (threes == null) return null;

            List<Card> result = threes.Take(3).ToList();
            
            // 找带牌 (找最小的单张或对子，且不拆炸弹)
            var others = hand.Where(c => c.Rank != threes.Key).OrderBy(c => c.GetLogicWeight()).ToList();
            
            if (others.Count > 0)
            {
                // 优先带单张
                result.Add(others[0]);
                // 如果规则允许带2张，可以再加一张
                // if (others.Count > 1) result.Add(others[1]); 
            }

            return result;
        }

        private static List<Card> FindSmallestPair(List<Card> hand)
        {
            var pair = hand.GroupBy(c => c.Rank)
                           .Where(g => g.Count() == 2) // 严格对子，不拆三张
                           .OrderBy(g => g.First().GetLogicWeight())
                           .FirstOrDefault();

            return pair?.ToList();
        }
    }
}