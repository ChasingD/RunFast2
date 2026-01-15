using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions; // 引入正则

namespace RunFast2.Scripts.Model
{
    [CreateAssetMenu(fileName = "CardSpriteAsset", menuName = "RunFast/CardSpriteAsset")]
    public class CardSpriteAsset : ScriptableObject
    {
        [Header("Card Sprites")]
        public List<Sprite> CardSprites; // 所有的牌面图片
        public Sprite CardBack; // 牌背

        // 辅助方法：根据花色和点数获取 Sprite
        public Sprite GetSprite(CardSuit suit, CardRank rank)
        {
            string suitName = suit.ToString();
            string rankName = GetRankName(rank);
            
            // 使用更精确的匹配逻辑
            // 假设 Sprite 名字格式为 "Spade_1", "Spade_10" 等
            // 我们需要确保 "1" 不会匹配到 "10", "11" 等
            
            foreach (var sprite in CardSprites)
            {
                // 1. 检查花色
                if (!sprite.name.Contains(suitName, System.StringComparison.OrdinalIgnoreCase)) continue;

                // 2. 检查点数 (使用正则或精确分割)
                // 假设命名规则是用下划线或空格分隔的，例如 "Spade_1.png"
                // 我们可以尝试提取数字部分进行比对
                
                // 简单方案：检查是否包含 rankName，并且 rankName 前后不是数字
                // 例如找 "1"，那么 "Spade_1" (ok), "Spade_10" (no), "Spade_11" (no)
                
                // 使用正则匹配独立的数字/字符
                // \b 表示单词边界，或者非数字字符边界
                // 注意：如果 rankName 是 "10"，Contains("1") 是 true，这是错误的。
                // 反之，找 "1"，Contains("1") 在 "10" 里也是 true。
                
                // 正则表达式：(?<!\d)rankName(?!\d)
                // 意思是：rankName 前面不能是数字，后面也不能是数字
                if (System.Text.RegularExpressions.Regex.IsMatch(sprite.name, $@"(?<!\d){rankName}(?!\d)"))
                {
                    return sprite;
                }
            }
            
            return null;
        }

        private string GetRankName(CardRank rank)
        {
            // 将枚举转换为素材中可能使用的名字
            // 例如 CardRank.Three -> "3", CardRank.Ace -> "1" or "A"
            switch (rank)
            {
                case CardRank.Ace: return "1"; // 很多素材包用 1 代表 A
                case CardRank.Two: return "2";
                case CardRank.Three: return "3";
                case CardRank.Four: return "4";
                case CardRank.Five: return "5";
                case CardRank.Six: return "6";
                case CardRank.Seven: return "7";
                case CardRank.Eight: return "8";
                case CardRank.Nine: return "9";
                case CardRank.Ten: return "10";
                case CardRank.Jack: return "11"; // 或 "J"
                case CardRank.Queen: return "12"; // 或 "Q"
                case CardRank.King: return "13"; // 或 "K"
                default: return ((int)rank).ToString();
            }
        }
    }
}