using System;
using Mirror; // 引用 Mirror 命名空间

// 保持枚举不变
public enum CardSuit { Diamond = 1, Club = 2, Heart = 3, Spade = 4 }
public enum CardRank { Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10, Jack = 11, Queen = 12, King = 13, Ace = 14, Two = 15 }

// 改为 struct，Mirror 可以自动序列化简单的 struct
[Serializable]
public struct Card
{
    public CardSuit Suit;
    public CardRank Rank;
    // ID 可以通过属性动态获取，不需要传输字符串，节省流量
    public string ID => $"{Suit}_{Rank}"; 

    public Card(CardSuit suit, CardRank rank)
    {
        this.Suit = suit;
        this.Rank = rank;
    }
    
    // 逻辑权重
    public int GetLogicWeight() => (int)Rank;
    
    public override string ToString() => $"[{Suit} {Rank}]";
}