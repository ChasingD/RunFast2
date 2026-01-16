using System;
using System.Collections.Generic;
using Mirror;
using RunFast2.Scripts.Model;
using UnityEngine;
using RunFast2.Scripts.Services; // 引用 AuthService
using Cysharp.Threading.Tasks; // 引用 UniTask
using System.Linq; // 引用 Linq
using RunFast2.Scripts.Logic; // 引用 PokerRules

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

        // 新增：同步剩余牌数
        [SyncVar(hook = nameof(OnCardCountChanged))]
        public int RemainingCardCount = 0;

        // 新增：托管状态
        [SyncVar(hook = nameof(OnAutoPlayChanged))]
        public bool IsAutoPlay = false;

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
        public event Action OnInitialHandReceived; // 仅在发牌时触发 (播放动画)
        public event Action OnHandUpdated;         // 手牌变动时触发 (仅刷新)

        // 游戏事件 (RPC收到时触发)
        public static event Action<int, Card[]> OnOpponentPlayed; // 座位号, 牌
        public static event Action<int> OnOpponentPassed;
        public static event Action<int> OnGameWin; // (旧) 简单获胜通知，可保留兼容
        
        // 结算事件
        public static event Action<RoundResult> OnRoundFinished;
        public static event Action<GameTotalResult> OnGameFinished;
        public static event Action<PlayerTotalStats[]> OnScoreUpdated; // 新增：分数更新事件

        // 抢关事件
        public static event Action<bool> OnShowRobUI; // true=show, false=hide
        public static event Action<int> OnRobResult; // seatIndex of robber, or -1 if none

        // 托管事件
        public static event Action<CardPlayer> OnAutoPlayStateChanged;

        // 道具事件
        public static event Action<int, int, int> OnItemEffectTriggered; // sourceSeat, targetSeat, itemTypeId

        // ================== 4. 生命周期 (Lifecycle) ==================

        public override void OnStartClient()
        {
            base.OnStartClient();
            OnPlayerInfoUpdated?.Invoke(this);
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            // 断线重连检查：如果游戏正在进行中，请求同步状态
            if (PokerManager.Instance != null && 
                (PokerManager.Instance.CurrentState == GameState.Playing || 
                 PokerManager.Instance.CurrentState == GameState.Robbing))
            {
                Debug.Log("[Reconnection] 游戏进行中，请求同步状态...");
                CmdRequestGameState();
            }
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
            // 1. 检查目标座位是否被占用
            foreach (var p in FindObjectsOfType<CardPlayer>())
            {
                if (p.SeatIndex == seatID) 
                {
                    Debug.LogWarning($"座位 {seatID} 已经被占用了。");
                    return; 
                }
            }

            // 2. 如果玩家已经在其他座位上，先离开旧座位（逻辑上不需要额外操作，直接覆盖 SeatIndex 即可）
            // 但为了清晰，可以打印日志
            if (this.SeatIndex != -1)
            {
                Debug.Log($"玩家 {PlayerName} 从座位 {this.SeatIndex} 换到了 {seatID}");
            }

            // 如果名字为空（未登录），使用 netId 作为名字
            if (string.IsNullOrEmpty(name))
                this.PlayerName = $"Player {netId}";
            else
                this.PlayerName = name; 

            // 3. 更新座位和名字
            this.SeatIndex = seatID;
    
            // 4. 修改逻辑：坐下即准备
            this.IsReady = true; 
            
            // 5. 检查是否所有人都准备好了
            CheckAllReady();
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
            // 手动操作取消托管
            if (IsAutoPlay) IsAutoPlay = false;

            if (PokerManager.Instance != null)
            {
                PokerManager.Instance.OnPlayerPlayCard(this, cards);
            }
        }

        [Command]
        public void CmdPass()
        {
            // 手动操作取消托管
            if (IsAutoPlay) IsAutoPlay = false;

            if (PokerManager.Instance != null)
            {
                PokerManager.Instance.OnPlayerPass(this);
            }
        }

        [Command]
        public void CmdRobPass(bool wantToRob)
        {
            if (PokerManager.Instance != null)
            {
                PokerManager.Instance.OnPlayerRob(this, wantToRob);
            }
        }

        [Command]
        public void CmdToggleAutoPlay(bool isAuto)
        {
            IsAutoPlay = isAuto;
            // 如果开启托管且轮到自己，立即尝试出牌
            if (IsAutoPlay && PokerManager.Instance != null)
            {
                PokerManager.Instance.CheckAutoPlay(this);
            }
        }

        [Command]
        public void CmdUseItem(int targetSeatIndex, int itemTypeId)
        {
            // TODO: 这里可以添加验证逻辑，比如检查玩家是否有足够的金币或道具数量
            // 目前直接广播给所有客户端播放特效
            RpcOnItemUsed(SeatIndex, targetSeatIndex, itemTypeId);
        }

        [Command]
        public void CmdRequestGameState()
        {
            // 重新发送手牌
            if (ServerHand.Count > 0)
            {
                TargetRpcReceiveHand(connectionToClient, ServerHand.ToArray());
            }

            // 重新发送当前牌桌状态 (LastHand)
            if (PokerManager.Instance != null && PokerManager.Instance.LastHand != null)
            {
                // 模拟一次出牌通知，让客户端显示上一手牌
                // 注意：这里需要把 PokerHand 转回 Card[] 和 type
                // 并且发送者是 LastPlayerSeatIndex
                TargetRpcSyncTableState(connectionToClient, 
                    PokerManager.Instance.LastPlayerSeatIndex, 
                    PokerManager.Instance.LastHand.Cards.ToArray(), 
                    (int)PokerManager.Instance.LastHand.Type);
            }

            // 如果是抢关阶段，重新显示 UI
            if (PokerManager.Instance != null && PokerManager.Instance.CurrentState == GameState.Robbing)
            {
                TargetRpcShowRobUI(connectionToClient);
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
                    PokerManager.Instance.InitializeGame((NetworkManager.singleton as RunFastNetworkManager)!.PendingRoomSettings);
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
            // 1. 原有逻辑：接收并整理手牌
            MyHand.Clear();
            MyHand.AddRange(newCards);
            SortHand();
            Debug.Log($"我是玩家 {netId} (座位 {SeatIndex}), 收到了 {MyHand.Count} 张牌。");

            // --- 【新增修复代码】强制重置客户端的牌桌状态 ---
            if (PokerManager.Instance != null)
            {
                // 只有在非重连（新开局）时才重置，或者由 PokerManager 状态决定
                // 这里简单处理：如果收到了新手牌，通常意味着新的一局或者重连恢复
                // 如果是重连，后面会有 SyncTableState 来覆盖
                PokerManager.Instance.LastHand = null;       
                PokerManager.Instance.LastPlayerSeatIndex = -1; 
            }
            // -------------------------------------------

            // 触发初始发牌事件 (带动画)
            OnInitialHandReceived?.Invoke();
        }

        [TargetRpc]
        public void TargetRpcSyncTableState(NetworkConnection target, int lastSeatIndex, Card[] lastCards, int lastHandType)
        {
            // 专门用于重连同步牌桌状态
            if (PokerManager.Instance != null)
            {
                var newHand = new PokerHand((HandType)lastHandType, 0, new List<Card>(lastCards));
                PokerManager.Instance.LastHand = newHand;
                PokerManager.Instance.LastPlayerSeatIndex = lastSeatIndex;
                
                // 触发 UI 更新 (显示上一手牌)
                OnOpponentPlayed?.Invoke(lastSeatIndex, lastCards);
            }
        }

        [TargetRpc]
        public void TargetRpcShowRobUI(NetworkConnection target)
        {
            OnShowRobUI?.Invoke(true);
        }

        [ClientRpc]
        public void RpcOnPlayerPlayed(int seatIndex, Card[] cards, int handType)
        {
            // 使用 string.Join 打印出牌详情
            Debug.Log($"玩家 {seatIndex} 出牌: {cards.Length} 张 {string.Join(", ", cards.Select(c => c.ToString()))}");
            OnOpponentPlayed?.Invoke(seatIndex, cards);

            // --- 【新增修复代码 开始】 ---
            // 客户端收到出牌消息时，手动更新本地 PokerManager 的状态
            if (PokerManager.Instance != null)
            {
                // 修正：调用 PokerRules.AnalyzeHand 重新计算权重，避免 Weight=0 的问题
                var newHand = PokerRules.AnalyzeHand(new List<Card>(cards));
                
                PokerManager.Instance.LastHand = newHand;
                PokerManager.Instance.LastPlayerSeatIndex = seatIndex;
        
                Debug.Log($"[Client] 更新上一手牌归属: 座位 {seatIndex}, 牌型: {newHand}");
            }
            // --- 【新增修复代码 结束】 ---

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
            // 旧的简单通知，保留兼容
            Debug.Log($"游戏结束，赢家: {winnerSeat}");
            OnGameWin?.Invoke(winnerSeat);
        }

        [ClientRpc]
        public void RpcOnRoundFinished(RoundResult result)
        {
            // 移除单局结算弹窗逻辑，只保留日志或调试
            Debug.Log($"[Client] 本局结束，结算数据收到 (后台处理)。");
            OnRoundFinished?.Invoke(result); // 恢复触发事件，以便 HandViewController 播放音效

            // 上传单局数据 (仅本地玩家上传自己的)
            if (isLocalPlayer)
            {
                UploadMyRoundRecord(result).Forget();
            }
        }

        [ClientRpc]
        public void RpcOnGameFinished(GameTotalResult result)
        {
            Debug.Log($"[Client] 整场游戏结束，总结算数据收到。");
            OnGameFinished?.Invoke(result);
        }

        private async UniTaskVoid UploadMyRoundRecord(RoundResult result)
        {
            Debug.Log("[Upload] Starting UploadMyRoundRecord...");
            
            if (AuthService.Instance == null)
            {
                Debug.LogError("[Upload] AuthService.Instance is null");
                return;
            }
            
            if (AuthService.Instance.supabaseManager == null)
            {
                Debug.LogError("[Upload] AuthService.Instance.supabaseManager is null");
                return;
            }

            if (!AuthService.Instance.IsLoggedIn)
            {
                Debug.LogWarning("[Upload] User is not logged in. Skipping upload.");
                return;
            }

            // 找到自己的记录
            var myResult = result.PlayerResults.Find(r => r.SeatIndex == SeatIndex);
            if (myResult.Equals(default(PlayerRoundResult)))
            {
                Debug.LogError($"[Upload] Could not find result for seat {SeatIndex}");
                return;
            }

            Debug.Log($"[Upload] Found my result: ScoreChange={myResult.ScoreChange}, IsWinner={myResult.IsWinner}");

            // 构建 GameRecord
            var record = new GameRecord
            {
                UserId = AuthService.Instance.CurrentUser.Id,
                ScoreChange = myResult.ScoreChange,
                IsWinner = myResult.IsWinner,
                IsRobber = myResult.IsRobber,
                IsRobSuccess = myResult.IsRobSuccess,
                IsReverseSuccess = false, // 暂未实现反关逻辑
                BombCount = 0, // 暂未在 PlayerRoundResult 中统计炸弹数，如果需要，需扩展 PlayerRoundResult
                CreatedAt = DateTime.UtcNow
            };

            Debug.Log("[Upload] Calling SupabaseManager.UploadGameRecord...");
            await AuthService.Instance.supabaseManager.UploadGameRecord(record);
        }

        [ClientRpc]
        public void RpcUpdateScores(PlayerTotalStats[] stats)
        {
            Debug.Log($"[Client] 收到分数更新。");
            OnScoreUpdated?.Invoke(stats);
        }

        [ClientRpc]
        public void RpcShowRobUI()
        {
            if (isLocalPlayer)
            {
                OnShowRobUI?.Invoke(true);
            }
        }

        [ClientRpc]
        public void RpcHideRobUI()
        {
            if (isLocalPlayer)
            {
                OnShowRobUI?.Invoke(false);
            }
        }

        [ClientRpc]
        public void RpcOnRobResult(int robberSeatIndex)
        {
            OnRobResult?.Invoke(robberSeatIndex);
        }

        [ClientRpc]
        public void RpcOnItemUsed(int sourceSeat, int targetSeat, int itemTypeId)
        {
            Debug.Log($"[Item] Player {sourceSeat} used item {(ItemType)itemTypeId} on Player {targetSeat}");
            OnItemEffectTriggered?.Invoke(sourceSeat, targetSeat, itemTypeId);
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
             // 触发手牌更新事件 (无动画)
             OnHandUpdated?.Invoke(); 
        }

        void OnSeatChanged(int oldVal, int newVal)
        {
            SeatIndex = newVal;
            OnPlayerInfoUpdated?.Invoke(this);
        }
        
        void OnCardCountChanged(int oldVal, int newVal)
        {
            RemainingCardCount = newVal;
            OnPlayerInfoUpdated?.Invoke(this);
        }

        void OnReadyChanged(bool oldVal, bool newVal)
        {
            IsReady = newVal;
            OnPlayerInfoUpdated?.Invoke(this);
        }

        void OnAutoPlayChanged(bool oldVal, bool newVal)
        {
            IsAutoPlay = newVal;
            OnAutoPlayStateChanged?.Invoke(this);
        }
    }
}