using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace RunFast2.Scripts.Network
{
    public class CardPlayer : NetworkBehaviour
    {
        // ================== 1. 游戏状态同步 (Lobby/Room Logic) ==================
    
        [Header("Room State")]
        [SyncVar(hook = nameof(OnSeatChanged))]
        public int SeatIndex = -1; // -1 代表未入座，0-2 代表座位号

        [SyncVar(hook = nameof(OnReadyChanged))]
        public bool IsReady = false;

        [SyncVar]
        public string PlayerName = "Unknown";

        // ================== 2. 游戏数据 (Gameplay Data) ==================

        // 客户端本地手牌
        public List<Card> MyHand = new List<Card>();

        // 服务器端手牌验证 (Authoritative)
        public readonly List<Card> ServerHand = new List<Card>();

        // ================== 3. 事件定义 (Events) ==================
    
        // UI 更新事件
        public static event Action<CardPlayer> OnPlayerInfoUpdated;
        public static event Action<CardPlayer> OnPlayerLeft;
    
        // 收到手牌事件
        public event Action OnHandReceived; 

        // 游戏事件 (RPC收到时触发)
        public static event Action<int, Card[]> OnOpponentPlayed; // 座位号, 牌
        public static event Action<int> OnOpponentPassed;
        public static event Action<int> OnGameWin;

        // ================== 4. 生命周期 (Lifecycle) ==================

        public override void OnStartClient()
        {
            base.OnStartClient();
            OnPlayerInfoUpdated?.Invoke(this);
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            OnPlayerLeft?.Invoke(this);
        }

        // ================== 5. 客户端 -> 服务器 命令 (Commands) ==================

        [Command]
        public void CmdSitDown(int seatID, string name)
        {
            foreach (var p in FindObjectsOfType<CardPlayer>())
            {
                if (p.SeatIndex == seatID) return; 
            }

            this.SeatIndex = seatID;
    
            if (string.IsNullOrEmpty(name))
                this.PlayerName = $"Player {netId}";
            else
                this.PlayerName = name; 

            this.IsReady = false; 
        }

        [Command]
        public void CmdToggleReady()
        {
            if (SeatIndex == -1) return;
            this.IsReady = !this.IsReady;
            CheckAllReady();
        }

        [Command]
        public void CmdPlayCard(Card[] cards)
        {
            if (PokerManager.Instance != null)
            {
                PokerManager.Instance.OnPlayerPlayCard(this, cards);
            }
        }

        [Command]
        public void CmdPass()
        {
            if (PokerManager.Instance != null)
            {
                PokerManager.Instance.OnPlayerPass(this);
            }
        }

        // ================== 6. 服务器逻辑 (Server Logic) ==================

        [Server]
        void CheckAllReady()
        {
            var allPlayers = FindObjectsOfType<CardPlayer>();
            int seatedCount = 0;
            int readyCount = 0;

            foreach (var p in allPlayers)
            {
                if (p.SeatIndex != -1)
                {
                    seatedCount++;
                    if (p.IsReady) readyCount++;
                }
            }

            if (seatedCount == 3 && readyCount == 3)
            {
                Debug.Log("所有玩家准备完毕，请求 PokerManager 发牌...");
                if (PokerManager.Instance != null)
                {
                    PokerManager.Instance.StartGame();
                }
                else
                {
                    Debug.LogError("PokerManager 实例未找到！");
                }
            }
        }

        // ================== 7. 服务器 -> 客户端 RPC (TargetRPC & Rpc) ==================

        [TargetRpc]
        public void TargetRpcReceiveHand(NetworkConnection target, Card[] newCards)
        {
            MyHand.Clear();
            MyHand.AddRange(newCards);
            SortHand();
            Debug.Log($"我是玩家 {netId} (座位 {SeatIndex}), 收到了 {MyHand.Count} 张牌。");
            OnHandReceived?.Invoke();
        }

        [ClientRpc]
        public void RpcOnPlayerPlayed(int seatIndex, Card[] cards, int handType)
        {
            // 如果是自己出的牌，因为本地预测/UI更新可能已经移除，这里确认同步
            // 如果是别人出的牌，UI显示动画
            Debug.Log($"玩家 {seatIndex} 出牌: {cards.Length} 张");
            OnOpponentPlayed?.Invoke(seatIndex, cards);

            // 如果是自己，需要确认本地手牌被扣除 (如果本地尚未扣除)
            if (SeatIndex == seatIndex && isLocalPlayer)
            {
                RemoveCardsFromLocalHand(cards);
            }
        }

        [ClientRpc]
        public void RpcOnPlayerPassed(int seatIndex)
        {
            Debug.Log($"玩家 {seatIndex} 不要");
            OnOpponentPassed?.Invoke(seatIndex);
        }

        [ClientRpc]
        public void RpcGameFinished(int winnerSeat)
        {
            Debug.Log($"游戏结束，赢家: {winnerSeat}");
            OnGameWin?.Invoke(winnerSeat);
        }

        // ================== 8. 辅助方法 & Hooks ==================

        void SortHand()
        {
            MyHand.Sort((a, b) => 
            {
                int weightA = a.GetLogicWeight();
                int weightB = b.GetLogicWeight();
                if (weightA != weightB) return weightB.CompareTo(weightA);
                return b.Suit.CompareTo(a.Suit);
            });
        }

        void RemoveCardsFromLocalHand(Card[] cards)
        {
             // 简单的移除逻辑：根据ID或值移除
             foreach(var card in cards)
             {
                 for(int i=0; i<MyHand.Count; i++)
                 {
                     if(MyHand[i].Suit == card.Suit && MyHand[i].Rank == card.Rank)
                     {
                         MyHand.RemoveAt(i);
                         break;
                     }
                 }
             }
             OnHandReceived?.Invoke(); // Refresh UI
        }

        void OnSeatChanged(int oldVal, int newVal)
        {
            SeatIndex = newVal;
            OnPlayerInfoUpdated?.Invoke(this);
        }

        void OnReadyChanged(bool oldVal, bool newVal)
        {
            IsReady = newVal;
            OnPlayerInfoUpdated?.Invoke(this);
        }
    }
}
