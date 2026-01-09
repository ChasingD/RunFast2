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

        // 本地存储玩家的手牌 (不需要 SyncVar，因为只有自己和服务器需要知道)
        public List<Card> MyHand = new List<Card>();

        // ================== 3. 事件定义 (Events) ==================
    
        // UI 更新事件
        public static event Action<CardPlayer> OnPlayerInfoUpdated;
        public static event Action<CardPlayer> OnPlayerLeft;
    
        // 收到手牌事件 (UI监听这个来显示牌)
        public event Action OnHandReceived; 

        // ================== 4. 生命周期 (Lifecycle) ==================

        public override void OnStartClient()
        {
            base.OnStartClient();
            // 客户端启动时，刷新一次 UI
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
            // 防抢座逻辑
            foreach (var p in FindObjectsOfType<CardPlayer>())
            {
                if (p.SeatIndex == seatID) return; 
            }

            this.SeatIndex = seatID;
    
            // 2. 这里不再使用 "Player {netId}"，而是使用传进来的 name
            // 为了防止名字为空，加个保底
            if (string.IsNullOrEmpty(name))
            {
                this.PlayerName = $"Player {netId}";
            }
            else
            {
                this.PlayerName = name; 
            }

            // 坐下时重置准备状态
            this.IsReady = false; 
        }

        [Command]
        public void CmdToggleReady()
        {
            if (SeatIndex == -1) return;
            this.IsReady = !this.IsReady;
        
            // 检查是否所有人都准备好了
            CheckAllReady();
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

            // 假设是3人局，且必须满3人才开始
            // 这里的数字3可以提取为常量或配置
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

        // ================== 7. 服务器 -> 客户端 RPC (TargetRPC) ==================

        /// <summary>
        /// 接收手牌：这是之前报错缺失的方法
        /// </summary>
        [TargetRpc]
        public void TargetRpcReceiveHand(NetworkConnection target, Card[] newCards)
        {
            MyHand.Clear();
            MyHand.AddRange(newCards);
        
            // 客户端理牌
            SortHand();

            Debug.Log($"我是玩家 {netId} (座位 {SeatIndex}), 收到了 {MyHand.Count} 张牌。");
        
            // 通知 UI (HandView) 显示这些牌
            OnHandReceived?.Invoke();
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