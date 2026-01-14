using Mirror;
using RunFast2.Scripts.Network;
using RunFast2.Scripts.View;
using UnityEngine;
using UnityEngine.UI;
using RunFast2.Scripts.Model;

namespace RunFast2.Scripts.Controller
{
    public class GameRoomUI : MonoBehaviour
    {
        [Header("References")]
        public SeatView[] Seats;     // 拖入 3 个 SeatView
        public Button ReadyButton;   // 屏幕下方的准备按钮
        // public Button StartGameButton; // (可选) 只有房主可见的开始按钮

        private int _myCurrentSeat = -1; // 记录本地玩家坐在哪

        void OnEnable()
        {
            // 订阅 CardPlayer 的静态事件
            CardPlayer.OnPlayerInfoUpdated += HandlePlayerUpdate;
            CardPlayer.OnPlayerLeft += HandlePlayerLeft;
            
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
            
            CardPlayer.OnOpponentPlayed -= HandlePlayerPlayed;
            CardPlayer.OnOpponentPassed -= HandlePlayerPassed;
            CardPlayer.OnRobResult -= HandleRobResult;
            PokerManager.OnTurnChangedEvent -= HandleTurnChanged;
            PokerManager.OnStateChangedEvent -= HandleStateChanged;
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
                    Seats[uiIndex].SetState_Occupied(player.PlayerName, player.IsReady, player.isLocalPlayer, player.RemainingCardCount);
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
                        Seats[uiIndex].SetState_Occupied(p.PlayerName, p.IsReady, p.isLocalPlayer, p.RemainingCardCount);
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