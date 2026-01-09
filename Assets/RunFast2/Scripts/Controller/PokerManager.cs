using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using RunFast2.Scripts.Network;

public class PokerManager : NetworkBehaviour
{
    // 单例模式，方便访问 (注意：网络游戏中单例要谨慎，确保场景切换处理得当)
    public static PokerManager Instance;

    private List<Card> _deck = new List<Card>();

    private void Awake()
    {
        Instance = this;
    }

    // 只有服务器能调用此方法开始游戏
    // 在 PokerManager.cs 中

    [Server]
    public void StartGame()
    {
        // 1. 获取所有玩家
        var allPlayers = FindObjectsOfType<CardPlayer>();
    
        // 2. 筛选出已经入座的玩家 (SeatIndex != -1) 并按座位号排序
        //    这步很重要！如果不排序，发牌顺序就会乱，座位0的人可能拿到了座位2的牌
        var seatedPlayers = allPlayers
            .Where(p => p.SeatIndex != -1)
            .OrderBy(p => p.SeatIndex)
            .ToArray();

        if (seatedPlayers.Length < 2) 
        {
            Debug.LogWarning("入座人数不足，无法开始");
            return;
        }

        InitDeck();
        ShuffleDeck();
    
        // 3. 给筛选出的 seatedPlayers 发牌
        DealCards(seatedPlayers);
    }

    [Server]
    void InitDeck()
    {
        _deck.Clear();
        foreach (CardSuit suit in System.Enum.GetValues(typeof(CardSuit)))
        {
            foreach (CardRank rank in System.Enum.GetValues(typeof(CardRank)))
            {
                // 这里可以剔除掉某些牌（如果规则需要）
                _deck.Add(new Card(suit, rank));
            }
        }
    }

    [Server]
    void ShuffleDeck()
    {
        System.Random rng = new System.Random();
        int n = _deck.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Card value = _deck[k];
            _deck[k] = _deck[n];
            _deck[n] = value;
        }
    }

    [Server]
    void DealCards(CardPlayer[] players)
    {
        // 临时存储每个人的牌
        List<List<Card>> hands = new List<List<Card>>();
        for (int i = 0; i < players.Length; i++) hands.Add(new List<Card>());

        // 轮流发牌
        for (int i = 0; i < _deck.Count; i++)
        {
            // 简单逻辑：如果是3人局，全发完；如果是2人局，可能会留牌
            // 这里假设 3人 均分
            int seatIndex = i % players.Length;
            hands[seatIndex].Add(_deck[i]);
        }

        // 将牌通过网络发送给对应的客户端
        for (int i = 0; i < players.Length; i++)
        {
            CardPlayer player = players[i];
            
            // 使用 TargetRpc 定向发送给特定玩家 (安全！玩家A收不到玩家B的牌数据)
            // 注意：TargetRpc 的第一个参数是 NetworkConnection，Mirror 会自动处理
            // 这里我们需要把 List 转成 Array，因为 Mirror 对 Array 的支持最基础且稳定
            player.TargetRpcReceiveHand(player.connectionToClient, hands[i].ToArray());
            
            Debug.Log($"服务器已向玩家 {player.netId} 发送了 {hands[i].Count} 张牌");
        }
    }
}