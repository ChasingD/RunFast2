using System;
using System.Collections.Generic;

namespace RunFast2.Scripts.Model
{
    public enum HandType
    {
        Invalid = 0,
        Single = 1,             // 单张
        Pair = 2,               // 对子
        Triplet = 3,            // 三张 (通常只在最后一手允许, 或特定规则)
        TripletWithOne = 4,     // 三带一 (跑得快通常不允许, 或者是最后一手)
        TripletWithTwo = 5,     // 三带二 (标准跑得快)
        Straight = 6,           // 顺子 (5张+)
        ConsecutivePairs = 7,   // 连对 (2对+)
        Airplane = 8,           // 飞机 (三顺 + 翅膀)
        Bomb = 9,               // 炸弹 (4张)
        FourWithThree = 10      // 四带三 (跑得快规则)
    }

    [Serializable]
    public class PokerHand
    {
        public HandType Type;
        public int Weight; // 用于比较大小的主值 (如对子的点数, 顺子的最大点数)
        public List<Card> Cards;

        public PokerHand(HandType type, int weight, List<Card> cards)
        {
            Type = type;
            Weight = weight;
            Cards = cards;
        }

        public override string ToString()
        {
            return $"{Type} (Weight: {Weight})";
        }
    }
}
