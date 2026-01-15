using Mirror;
using RunFast2.Scripts.Network;
using RunFast2.Scripts.View;
using UnityEngine;
using UnityEngine.UI;
using RunFast2.Scripts.Model;
using System.Linq;

namespace RunFast2.Scripts.Controller
{
    public class GameRoomUI : MonoBehaviour
    {
        [Header("References")]
        public SeatView[] Seats;     // 拖入 3 个 SeatView
        public Button ReadyButton;   // 屏幕下方的准备按钮
        // public Button StartGameButton; // (可选) 只有房主可见的开始按钮

        private int _myCurrentSeat = -1; // 记录本地玩家坐在哪
        private PlayerTotalStats[] _lastStats; // 缓存上一次的分数

        void OnEnable()
        {
            // 订阅 CardPlayer 的静态事件
            CardPlayer.OnPlayerInfoUpdated += HandlePlayerUpdate;
            CardPlayer.OnPlayerLeft += HandlePlayerLeft;
            CardPlayer.OnScoreUpdated += HandleScoreUpdate; // 订阅分数更新事件
            
            // 订阅出牌事件
            CardPlayer.OnOpponentPlayed += HandlePlayerPlayed;
            CardPlayer.OnOpponentPassed += HandlePlayerPassed;
            CardPlayer.OnRobResult += HandleRobResult;
            PokerManager.OnTurnChangedEvent += HandleTurnChanged;
            PokerManager.OnStateChangedEvent += HandleStateChanged;
        
            // 绑定椅子点击事件
            for (int i = 0; i < Seats.Length; i++)
            {
                // 修正：将 SeatView 自身的 SeatID 传递出去
                // SeatView 的 SeatID 应该在 Inspector 中手动设置为 0, 1, 2，代表 UI 的绝对位置
                Seats[i].OnSitClicked = OnSeatClicked; 
            }
        
            if (ReadyButton) ReadyButton.onClick.AddListener(OnReadyClicked);
            UpdateReadyButtonState();
        }

        void OnDisable()
        {
            CardPlayer.OnPlayerInfoUpdated -= HandlePlayerUpdate;
            CardPlayer.OnPlayerLeft -= HandlePlayerLeft;
            CardPlayer.OnScoreUpdated -= HandleScoreUpdate; // 取消订阅
            
            CardPlayer.OnOpponentPlayed -= HandlePlayerPlayed;
            CardPlayer.OnOpponentPassed -= HandlePlayerPassed;
            CardPlayer.OnRobResult -= HandleRobResult;
            PokerManager.OnTurnChangedEvent -= HandleTurnChanged;
            PokerManager.OnStateChangedEvent -= HandleStateChanged;
        }

        private void Update()
        {
            // 更新倒计时
            UpdateTurnTimer();
        }

        void UpdateTurnTimer()
        {
            if (PokerManager.Instance == null) return;

            // 只有在 Playing 或 Robbing 状态下才更新
            if (PokerManager.Instance.CurrentState != GameState.Playing && 
                PokerManager.Instance.CurrentState != GameState.Robbing)
            {
                // 隐藏所有倒计时
                foreach (var seat in Seats) seat.SetActiveState(false);
                return;
            }

            double endTime = PokerManager.Instance.TurnEndTime;
            double remaining = (float)(endTime - NetworkTime.time);
            
            // 找到当前行动的玩家
            int currentPlayerServerIndex = PokerManager.Instance.CurrentPlayerIndex;
            
            // 如果是抢关阶段，可能没有 CurrentPlayerIndex (如果是大家一起抢)，或者有特定的 RobberSeatIndex
            // 但根据 PokerManager 逻辑，Robbing 阶段 CurrentPlayerIndex 是 -1，大家一起抢
            // 如果是大家一起抢，那么每个人都应该显示倒计时？或者只显示自己的？
            // 之前的逻辑是 HandViewController 显示自己的。
            // 这里我们只处理 Playing 阶段的轮流出牌倒计时。
            
            if (PokerManager.Instance.CurrentState == GameState.Playing)
            {
                if (currentPlayerServerIndex != -1)
                {
                    int uiIndex = GetUIIndex(currentPlayerServerIndex);
                    
                    // 更新所有座位的状态
                    for (int i = 0; i < Seats.Length; i++)
                    {
                        bool isActive = (i == uiIndex);
                        Seats[i].SetActiveState(isActive);
                        if (isActive)
                        {
                            Seats[i].UpdateTimer((float)remaining);
                        }
                    }
                }
            }
            else
            {
                // Robbing 阶段，暂时隐藏座位上的倒计时，使用 HandViewController 的中央倒计时
                foreach (var seat in Seats) seat.SetActiveState(false);
            }
        }

        // ================== 辅助方法：座位映射 ==================

        /// <summary>
        /// 将服务器座位号转换为 UI 索引
        /// </summary>
        int GetUIIndex(int serverSeatIndex)
        {
            if (serverSeatIndex < 0 || serverSeatIndex >= Seats.Length) return -1;

            // 如果游戏未开始，或者我还没坐下，保持绝对视角
            if (PokerManager.Instance == null || 
                PokerManager.Instance.CurrentState == GameState.Waiting ||
                _myCurrentSeat == -1)
            {
                return serverSeatIndex;
            }

            // 游戏开始后，切换到相对视角
            int totalSeats = Seats.Length;
            return (serverSeatIndex - _myCurrentSeat + totalSeats) % totalSeats;
        }

        /// <summary>
        /// 将 UI 索引转换为服务器座位号
        /// </summary>
        int GetServerSeatIndex(int uiIndex)
        {
            if (uiIndex < 0 || uiIndex >= Seats.Length) return -1;

            // 如果游戏未开始，或者我还没坐下，保持绝对视角
            if (PokerManager.Instance == null || 
                PokerManager.Instance.CurrentState == GameState.Waiting ||
                _myCurrentSeat == -1)
            {
                return uiIndex;
            }

            // 游戏开始后，切换到相对视角
            int totalSeats = Seats.Length;
            return (uiIndex + _myCurrentSeat) % totalSeats;
        }

        // ================== 逻辑处理 ==================

        // 1. 当某个座位被点击时 (传入的是 UI Index)
        void OnSeatClicked(int uiIndex)
        {
            // 获取本地玩家对象
            var localPlayer = NetworkClient.localPlayer?.GetComponent<CardPlayer>();
        
            if (localPlayer != null)
            {
                // 将 UI 索引转换为真实的服务器座位号
                int targetServerSeat = GetServerSeatIndex(uiIndex);

                // 发送坐下请求
                string myName = UserSession.CurrentPlayerName;
                localPlayer.CmdSitDown(targetServerSeat, myName);
            }
        }

        // 2. 点击准备按钮
        void OnReadyClicked()
        {
            var localPlayer = NetworkClient.localPlayer?.GetComponent<CardPlayer>();
            if (localPlayer != null)
            {
                localPlayer.CmdToggleReady();
            }
        }

        // 3. 核心：处理任何玩家状态更新
        void HandlePlayerUpdate(CardPlayer player)
        {
            // 如果是本地玩家，先更新本地记录
            if (player.isLocalPlayer)
            {
                bool seatChanged = (_myCurrentSeat != player.SeatIndex);
                _myCurrentSeat = player.SeatIndex;
                UpdateReadyButtonState();

                // 如果我的座位变了，需要刷新所有人的位置
                if (seatChanged)
                {
                    RefreshAllSeats();
                    return; // RefreshAllSeats 会处理所有人的显示，这里直接返回
                }
            }

            // 如果不是本地玩家座位变化，或者本地玩家状态更新（如准备状态），
            // 只更新单个座位即可，避免不必要的全场刷新
            if (player.SeatIndex >= 0 && player.SeatIndex < Seats.Length)
            {
                int uiIndex = GetUIIndex(player.SeatIndex);
                if (uiIndex != -1)
                {
                    // 尝试从缓存中获取分数
                    int score = 0;
                    if (_lastStats != null)
                    {
                        var stats = _lastStats.FirstOrDefault(s => s.SeatIndex == player.SeatIndex);
                        if (!stats.Equals(default(PlayerTotalStats)))
                        {
                            score = stats.TotalScore;
                        }
                    }
                    Seats[uiIndex].SetState_Occupied(player.PlayerName, player.IsReady, player.isLocalPlayer, player.RemainingCardCount, score);
                }
            }
            else
            {
                // 玩家站起来了，需要刷新全场来清理他的旧座位
                RefreshAllSeats();
            }
        }
    
        // 4. 处理玩家离开（掉线/退出）
        void HandlePlayerLeft(CardPlayer player)
        {
            RefreshAllSeats();
        }

        // 5. 处理出牌显示
        void HandlePlayerPlayed(int seatIndex, Card[] cards)
        {
            int uiIndex = GetUIIndex(seatIndex);
            Debug.Log($"[GameRoomUI] 收到出牌通知: ServerSeat {seatIndex} -> UISeat {uiIndex}, {cards.Length} 张牌");
            
            if (uiIndex >= 0 && uiIndex < Seats.Length)
            {
                Seats[uiIndex].ShowPlayedCards(cards);
            }
        }

        // 6. 处理过牌显示
        void HandlePlayerPassed(int seatIndex)
        {
            int uiIndex = GetUIIndex(seatIndex);
            Debug.Log($"[GameRoomUI] 收到过牌通知: ServerSeat {seatIndex} -> UISeat {uiIndex}");
            
            if (uiIndex >= 0 && uiIndex < Seats.Length)
            {
                Seats[uiIndex].ShowActionText("不要");
            }
        }

        // 7. 处理回合切换 (清理新回合玩家的出牌区)
        void HandleTurnChanged(int currentSeatIndex)
        {
            int uiIndex = GetUIIndex(currentSeatIndex);
            
            if (uiIndex >= 0 && uiIndex < Seats.Length)
            {
                Seats[uiIndex].ClearPlayedCards();
            }
            
            // 触发一次倒计时更新
            UpdateTurnTimer();
        }

        // 8. 处理状态切换 (新的一局开始时清空所有)
        void HandleStateChanged(GameState newState)
        {
            // 游戏开始时，刷新所有座位以切换到相对视角
            if (newState == GameState.Playing || newState == GameState.Robbing)
            {
                RefreshAllSeats();
            }
            
            // 新一局开始，清空所有人的出牌区
            if (newState == GameState.Playing || newState == GameState.Robbing || newState == GameState.RoundFinished)
            {
                foreach (var seat in Seats)
                {
                    seat.ClearPlayedCards();
                }
            }
            
            // 游戏开始后，隐藏所有 ReadyIcon
            if (newState == GameState.Playing || newState == GameState.Robbing)
            {
                foreach (var seat in Seats)
                {
                    if (seat.ReadyIcon) seat.ReadyIcon.gameObject.SetActive(false);
                }
            }
        }

        // 9. 处理抢关结果显示
        void HandleRobResult(int robberSeatIndex)
        {
            if (robberSeatIndex != -1)
            {
                int uiIndex = GetUIIndex(robberSeatIndex);
                if (uiIndex >= 0 && uiIndex < Seats.Length)
                {
                    Seats[uiIndex].ShowActionText("抢关成功");
                }
            }
        }

        // 10. 处理分数更新
        void HandleScoreUpdate(PlayerTotalStats[] newStats)
        {
            foreach (var stat in newStats)
            {
                int uiIndex = GetUIIndex(stat.SeatIndex);
                if (uiIndex >= 0 && uiIndex < Seats.Length)
                {
                    int oldScore = 0;
                    if (_lastStats != null)
                    {
                        var oldStat = _lastStats.FirstOrDefault(s => s.SeatIndex == stat.SeatIndex);
                        if (!oldStat.Equals(default(PlayerTotalStats)))
                        {
                            oldScore = oldStat.TotalScore;
                        }
                    }
                    
                    Seats[uiIndex].UpdateScore(stat.TotalScore, stat.TotalScore - oldScore);
                }
            }
            _lastStats = newStats; // 更新缓存
        }

        // ================== 辅助方法 ==================

        void RefreshAllSeats()
        {
            // 1. 先全部清空
            foreach (var seat in Seats) seat.SetState_Empty();

            // 2. 找到所有在线玩家填进去
            var allPlayers = FindObjectsOfType<CardPlayer>();
            foreach (var p in allPlayers)
            {
                if (p.SeatIndex >= 0 && p.SeatIndex < Seats.Length)
                {
                    int uiIndex = GetUIIndex(p.SeatIndex);
                    if (uiIndex != -1)
                    {
                        int score = 0;
                        if (_lastStats != null)
                        {
                            var stats = _lastStats.FirstOrDefault(s => s.SeatIndex == p.SeatIndex);
                            if (!stats.Equals(default(PlayerTotalStats))) score = stats.TotalScore;
                        }
                        
                        // 只有在 Waiting 状态下才显示 ReadyIcon
                        bool showReady = p.IsReady && (PokerManager.Instance == null || PokerManager.Instance.CurrentState == GameState.Waiting);
                        
                        Seats[uiIndex].SetState_Occupied(p.PlayerName, showReady, p.isLocalPlayer, p.RemainingCardCount, score);
                    }
                }
            }
        
            UpdateAllSeatsInteractable();
        }

        void UpdateReadyButtonState()
        {
            // 只有坐下后，才能看到/点击准备按钮
            if (ReadyButton) ReadyButton.gameObject.SetActive(_myCurrentSeat != -1);
        
            // 获取本地玩家更新按钮文字（准备/取消）
            var localPlayer = NetworkClient.localPlayer?.GetComponent<CardPlayer>();
            if (localPlayer != null && ReadyButton)
            {
                var text = ReadyButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if(text) text.text = localPlayer.IsReady ? "取消准备" : "准备";
            }
        }

        void UpdateAllSeatsInteractable()
        {
            // 始终允许点击空座位（为了换座位）
            foreach (var seat in Seats)
            {
                seat.SetInteractable(true);
            }
        }
    }
}