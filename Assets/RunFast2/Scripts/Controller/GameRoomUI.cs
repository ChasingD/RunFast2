using Mirror;
using RunFast2.Scripts.Network;
using RunFast2.Scripts.View;
using UnityEngine;
using UnityEngine.UI;
using RunFast2.Scripts.Model;
using System.Linq;
using System.Collections.Generic;

namespace RunFast2.Scripts.Controller
{
    public class GameRoomUI : MonoBehaviour
    {
        [Header("Seat References")]
        public LobbySeatView[] LobbySeats; // 准备阶段座位 (绝对位置 0,1,2)
        public GameSeatView[] GameSeats;   // 游戏阶段座位 (相对位置 0=Bottom, 1=Right, 2=Left)

        [Header("UI Controls")]
        public Button ReadyButton;   // 屏幕下方的准备按钮
        // public Button StartGameButton; // (可选) 只有房主可见的开始按钮

        [Header("Item System")]
        public GameObject ItemMenuPrefab; // 道具菜单预制体
        private GameObject _activeItemMenu;

        private int _myCurrentSeat = -1; // 记录本地玩家坐在哪
        private PlayerTotalStats[] _lastStats; // 缓存上一次的分数

        void OnEnable()
        {
            // 订阅 CardPlayer 的静态事件
            CardPlayer.OnPlayerInfoUpdated += HandlePlayerUpdate;
            CardPlayer.OnPlayerLeft += HandlePlayerLeft;
            CardPlayer.OnScoreUpdated += HandleScoreUpdate; // 订阅分数更新事件
            CardPlayer.OnItemEffectTriggered += HandleItemEffect; // 订阅道具事件
            
            // 订阅出牌事件
            CardPlayer.OnOpponentPlayed += HandlePlayerPlayed;
            CardPlayer.OnOpponentPassed += HandlePlayerPassed;
            CardPlayer.OnRobResult += HandleRobResult;
            PokerManager.OnTurnChangedEvent += HandleTurnChanged;
            PokerManager.OnStateChangedEvent += HandleStateChanged;
        
            // 绑定 LobbySeat 点击事件
            for (int i = 0; i < LobbySeats.Length; i++)
            {
                int serverSeatIndex = i; // LobbySeat 的索引直接对应服务器座位
                LobbySeats[i].OnSitClicked = (id) => OnLobbySeatClicked(serverSeatIndex);
            }

            // 绑定 GameSeat 头像点击事件
            for (int i = 0; i < GameSeats.Length; i++)
            {
                int uiIndex = i;
                GameSeats[i].OnAvatarClicked = (id) => OnGameAvatarClicked(uiIndex);
            }
        
            if (ReadyButton) ReadyButton.onClick.AddListener(OnReadyClicked);
            
            // 初始状态刷新
            RefreshAllSeats();
        }

        void OnDisable()
        {
            CardPlayer.OnPlayerInfoUpdated -= HandlePlayerUpdate;
            CardPlayer.OnPlayerLeft -= HandlePlayerLeft;
            CardPlayer.OnScoreUpdated -= HandleScoreUpdate; 
            CardPlayer.OnItemEffectTriggered -= HandleItemEffect;
            
            CardPlayer.OnOpponentPlayed -= HandlePlayerPlayed;
            CardPlayer.OnOpponentPassed -= HandlePlayerPassed;
            CardPlayer.OnRobResult -= HandleRobResult;
            PokerManager.OnTurnChangedEvent -= HandleTurnChanged;
            PokerManager.OnStateChangedEvent -= HandleStateChanged;
        }

        private void Update()
        {
            UpdateTurnTimer();
        }

        // ================== 核心逻辑：状态切换与刷新 ==================

        void RefreshAllSeats()
        {
            if (PokerManager.Instance == null) return;

            bool isGameStarted = PokerManager.Instance.CurrentState == GameState.Playing || 
                                 PokerManager.Instance.CurrentState == GameState.Robbing;

            // 1. 控制两组座位的显隐
            foreach (var seat in LobbySeats) seat.gameObject.SetActive(!isGameStarted);
            foreach (var seat in GameSeats) seat.gameObject.SetActive(isGameStarted);

            // 2. 更新 ReadyButton 状态 (只在准备阶段显示)
            if (ReadyButton) ReadyButton.gameObject.SetActive(!isGameStarted && _myCurrentSeat != -1);

            // 3. 填充数据
            var allPlayers = FindObjectsOfType<CardPlayer>();

            if (!isGameStarted)
            {
                // --- 准备阶段 (绝对视角) ---
                foreach (var seat in LobbySeats) seat.SetState_Empty();

                foreach (var p in allPlayers)
                {
                    if (p.SeatIndex >= 0 && p.SeatIndex < LobbySeats.Length)
                    {
                        LobbySeats[p.SeatIndex].SetState_Occupied(p.PlayerName, p.IsReady, p.isLocalPlayer);
                    }
                }
                
                // 更新空座位的交互性 (如果我已经坐下，其他空位不可点？或者允许换座)
                // 这里允许换座
                foreach (var seat in LobbySeats) seat.SetInteractable(true);
            }
            else
            {
                // --- 游戏阶段 (相对视角) ---
                // 先隐藏所有 GameSeat (SetState_Occupied 会激活它们)
                foreach (var seat in GameSeats) seat.gameObject.SetActive(false);

                foreach (var p in allPlayers)
                {
                    if (p.SeatIndex >= 0)
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
                            
                            GameSeats[uiIndex].SetState_Occupied(p.PlayerName, p.isLocalPlayer, p.RemainingCardCount, score);
                        }
                    }
                }
            }
        }

        // ================== 辅助方法：座位映射 (仅用于游戏阶段) ==================

        int GetUIIndex(int serverSeatIndex)
        {
            // 游戏阶段必须有本地玩家座位才能计算相对位置
            // 如果我是旁观者 (_myCurrentSeat == -1)，则保持绝对视角 (0->0, 1->1, 2->2)
            if (_myCurrentSeat == -1) return serverSeatIndex;

            int totalSeats = 3; // 假设3人局
            // 相对位置公式: (Target - My + Total) % Total
            return (serverSeatIndex - _myCurrentSeat + totalSeats) % totalSeats;
        }

        int GetServerSeatIndex(int uiIndex)
        {
            if (_myCurrentSeat == -1) return uiIndex;
            int totalSeats = 3;
            return (uiIndex + _myCurrentSeat) % totalSeats;
        }

        // ================== 事件处理 ==================

        void OnLobbySeatClicked(int serverSeatIndex)
        {
            var localPlayer = NetworkClient.localPlayer?.GetComponent<CardPlayer>();
            if (localPlayer != null)
            {
                string myName = UserSession.CurrentPlayerName;
                localPlayer.CmdSitDown(serverSeatIndex, myName);
            }
        }

        void OnGameAvatarClicked(int uiIndex)
        {
            // 点击游戏中的头像，弹出道具菜单
            int targetServerSeat = GetServerSeatIndex(uiIndex);
            var localPlayer = NetworkClient.localPlayer?.GetComponent<CardPlayer>();
            
            // 不能对自己使用道具
            if (localPlayer != null && targetServerSeat != localPlayer.SeatIndex)
            {
                ShowItemMenu(uiIndex, targetServerSeat);
            }
        }

        void OnReadyClicked()
        {
            var localPlayer = NetworkClient.localPlayer?.GetComponent<CardPlayer>();
            if (localPlayer != null)
            {
                localPlayer.CmdToggleReady();
            }
        }

        void HandlePlayerUpdate(CardPlayer player)
        {
            if (player.isLocalPlayer)
            {
                _myCurrentSeat = player.SeatIndex;
                if (ReadyButton)
                {
                    var text = ReadyButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if(text) text.text = player.IsReady ? "取消准备" : "准备";
                }
            }
            RefreshAllSeats();
        }

        void HandleStateChanged(GameState newState)
        {
            RefreshAllSeats();
            
            // 新局开始清理
            if (newState == GameState.Playing || newState == GameState.Robbing)
            {
                foreach (var seat in GameSeats) seat.ClearPlayedCards();
            }
        }

        // ... 其他处理逻辑 (HandlePlayerPlayed, HandleTurnChanged 等) ...
        // 注意：这些逻辑现在需要操作 GameSeats 数组，而不是 LobbySeats

        void HandlePlayerPlayed(int seatIndex, Card[] cards)
        {
            // 只有游戏阶段才显示出牌
            if (PokerManager.Instance.CurrentState != GameState.Playing && 
                PokerManager.Instance.CurrentState != GameState.Robbing) return;

            int uiIndex = GetUIIndex(seatIndex);
            if (uiIndex >= 0 && uiIndex < GameSeats.Length)
            {
                GameSeats[uiIndex].ShowPlayedCards(cards);
            }
        }

        void HandlePlayerPassed(int seatIndex)
        {
            if (PokerManager.Instance.CurrentState != GameState.Playing) return;

            int uiIndex = GetUIIndex(seatIndex);
            if (uiIndex >= 0 && uiIndex < GameSeats.Length)
            {
                GameSeats[uiIndex].ShowActionText("不要");
            }
        }

        void HandleTurnChanged(int currentSeatIndex)
        {
            int uiIndex = GetUIIndex(currentSeatIndex);
            if (uiIndex >= 0 && uiIndex < GameSeats.Length)
            {
                GameSeats[uiIndex].ClearPlayedCards();
            }
            UpdateTurnTimer();
        }

        void HandleRobResult(int robberSeatIndex)
        {
            if (robberSeatIndex != -1)
            {
                int uiIndex = GetUIIndex(robberSeatIndex);
                if (uiIndex >= 0 && uiIndex < GameSeats.Length)
                {
                    GameSeats[uiIndex].ShowActionText("抢关成功");
                }
            }
        }

        void HandleScoreUpdate(PlayerTotalStats[] newStats)
        {
            _lastStats = newStats;
            foreach (var stat in newStats)
            {
                int uiIndex = GetUIIndex(stat.SeatIndex);
                if (uiIndex >= 0 && uiIndex < GameSeats.Length)
                {
                    // 计算变化量逻辑略，直接更新总分
                    GameSeats[uiIndex].UpdateScore(stat.TotalScore, 0); 
                }
            }
        }

        void HandleItemEffect(int sourceSeat, int targetSeat, int itemTypeId)
        {
            // 播放特效逻辑
        }

        void HandlePlayerLeft(CardPlayer player)
        {
            RefreshAllSeats();
        }

        void UpdateTurnTimer()
        {
            if (PokerManager.Instance == null) return;
            if (PokerManager.Instance.CurrentState != GameState.Playing)
            {
                foreach (var seat in GameSeats) seat.SetActiveState(false);
                return;
            }

            double remaining = PokerManager.Instance.TurnEndTime - NetworkTime.time;
            int currentSeat = PokerManager.Instance.CurrentPlayerIndex;
            
            if (currentSeat != -1)
            {
                int uiIndex = GetUIIndex(currentSeat);
                for (int i = 0; i < GameSeats.Length; i++)
                {
                    bool isActive = (i == uiIndex);
                    GameSeats[i].SetActiveState(isActive);
                    if (isActive) GameSeats[i].UpdateTimer((float)remaining);
                }
            }
        }

        void ShowItemMenu(int uiIndex, int targetServerSeat)
        {
            if (_activeItemMenu != null) Destroy(_activeItemMenu);
            if (ItemMenuPrefab == null) return;

            _activeItemMenu = Instantiate(ItemMenuPrefab, transform);
            _activeItemMenu.transform.position = GameSeats[uiIndex].transform.position;

            // 使用 ItemMenu 脚本初始化
            var menu = _activeItemMenu.GetComponent<ItemMenu>();
            if (menu != null)
            {
                menu.Initialize(targetServerSeat, OnItemClicked);
            }
        }

        void OnItemClicked(int targetSeat, ItemType type)
        {
            var localPlayer = NetworkClient.localPlayer?.GetComponent<CardPlayer>();
            if (localPlayer) localPlayer.CmdUseItem(targetSeat, (int)type);
        }
    }
}