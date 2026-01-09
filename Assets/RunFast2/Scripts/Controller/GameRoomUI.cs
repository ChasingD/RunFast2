using Mirror;
using RunFast2.Scripts.Network;
using RunFast2.Scripts.View;
using UnityEngine;
using UnityEngine.UI;

namespace RunFast2.Scripts.Controller
{
    public class GameRoomUI : MonoBehaviour
    {
        [Header("References")]
        public SeatView[] Seats;     // 拖入 3 个 SeatView
        public Button ReadyButton;   // 屏幕下方的准备按钮
        public Button StartGameButton; // (可选) 只有房主可见的开始按钮

        private int _myCurrentSeat = -1; // 记录本地玩家坐在哪

        void OnEnable()
        {
            // 订阅 CardPlayer 的静态事件
            CardPlayer.OnPlayerInfoUpdated += HandlePlayerUpdate;
            CardPlayer.OnPlayerLeft += HandlePlayerLeft;
        
            // 绑定椅子点击事件
            foreach(var seat in Seats)
            {
                seat.OnSitClicked = OnSeatClicked; // 委托绑定
            }
        
            ReadyButton.onClick.AddListener(OnReadyClicked);
            UpdateReadyButtonState();
        }

        void OnDisable()
        {
            CardPlayer.OnPlayerInfoUpdated -= HandlePlayerUpdate;
            CardPlayer.OnPlayerLeft -= HandlePlayerLeft;
        }

        // ================== 逻辑处理 ==================

        // 1. 当某个座位被点击时
        void OnSeatClicked(int seatID)
        {
            // 获取本地玩家对象
            var localPlayer = NetworkClient.localPlayer?.GetComponent<CardPlayer>();
        
            // 如果我还没坐下，就发送命令坐这个位置
            if (localPlayer != null && localPlayer.SeatIndex == -1)
            {
                // 从全局变量获取名字，传给服务器
                string myName = UserSession.CurrentPlayerName;
                localPlayer.CmdSitDown(seatID, myName);
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
            // A. 如果这个玩家坐下了 (SeatIndex 0-2)
            if (player.SeatIndex >= 0 && player.SeatIndex < Seats.Length)
            {
                var seatUI = Seats[player.SeatIndex];
                seatUI.SetState_Occupied(player.PlayerName, player.IsReady, player.isLocalPlayer);
            }
        
            // B. 如果我是本地玩家，更新我的状态记录
            if (player.isLocalPlayer)
            {
                _myCurrentSeat = player.SeatIndex;
                UpdateReadyButtonState();
                UpdateAllSeatsInteractable();
            }
        
            // C. 处理一种特殊情况：玩家从座位A换到了座位B (虽然跑得快一般不让换，但逻辑要健壮)
            // 遍历所有座位，如果座位上的人不是当前记录的人，就重置为空
            RefreshAllSeats();
        }
    
        // 4. 处理玩家离开（掉线/退出）
        void HandlePlayerLeft(CardPlayer player)
        {
            RefreshAllSeats();
        }

        // ================== 辅助方法 ==================

        void RefreshAllSeats()
        {
            // 这是一个暴力但安全的刷新方法：先全重置，再填人
            // 1. 全部设为空
            foreach (var seat in Seats) seat.SetState_Empty();

            // 2. 找到所有在线玩家填进去
            var allPlayers = FindObjectsOfType<CardPlayer>();
            foreach (var p in allPlayers)
            {
                if (p.SeatIndex >= 0 && p.SeatIndex < Seats.Length)
                {
                    Seats[p.SeatIndex].SetState_Occupied(p.PlayerName, p.IsReady, p.isLocalPlayer);
                }
            }
        
            UpdateAllSeatsInteractable();
        }

        void UpdateReadyButtonState()
        {
            // 只有坐下后，才能看到/点击准备按钮
            ReadyButton.gameObject.SetActive(_myCurrentSeat != -1);
        
            // 获取本地玩家更新按钮文字（准备/取消）
            var localPlayer = NetworkClient.localPlayer?.GetComponent<CardPlayer>();
            if (localPlayer != null)
            {
                var text = ReadyButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if(text) text.text = localPlayer.IsReady ? "取消准备" : "准备";
            }
        }

        void UpdateAllSeatsInteractable()
        {
            // 如果我已经坐下了，其他空座位的“入座”按钮应该不可点
            // 如果我没坐下，空座位可点
            bool canSit = (_myCurrentSeat == -1);

            foreach (var seat in Seats)
            {
                seat.SetInteractable(canSit);
            }
        }
    }
}