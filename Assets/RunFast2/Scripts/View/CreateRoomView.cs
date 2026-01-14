using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.UI;
using RunFast2.Scripts.Model;
using RunFast2.Scripts.Network;
using PlayMode = RunFast2.Scripts.Model.PlayMode;

namespace RunFast2.Scripts.View
{
    public class CreateRoomView : MonoBehaviour
    {
        [Header("局数选择 (Toggle Group)")]
        // 将这三个 Toggle 放到同一个 ToggleGroup 下
        public Toggle toggleRounds3;
        public Toggle toggleRounds9;
        public Toggle toggleRounds18;
        public Toggle toggleRounds36;

        [Header("玩法模式 (Toggle Group)")]
        // 将这两个 Toggle 放到另一个 ToggleGroup 下
        public Toggle toggleModeMust;      // 有出必出
        public Toggle toggleModeOptional;  // 非必出

        [Header("先手规则 (Toggle Group)")]
        // 将这三个 Toggle 放到另一个 ToggleGroup 下
        public Toggle toggleTurnHeart3;    // 红桃3先出
        public Toggle toggleTurnWinner;    // 赢家先出
        public Toggle toggleTurnRotate;    // 轮流先出

        [Header("炸弹分数 (Toggle Group)")]
        public Toggle toggleBomb5;
        public Toggle toggleBomb10;

        [Header("普通规则开关 (独立 Toggle)")]
        public Toggle toggleShowHandCount; // 显示剩余张数
        public Toggle togglePayForAll;     // 放走包赔
        public Toggle toggleThreeAsBomb;   // 三A算炸
        public Toggle toggleNoLoseOnSingle;// 报单不输
        public Toggle toggleRobPass;       // 抢关

        [Header("操作按钮")]
        public Button createButton;
        public Button searchRoomButton;
        public Button closeButton;

        public GameObject roomPanel;

        public NetworkDiscovery networkDiscovery;
        private void Start()
        {
            if (createButton) createButton.onClick.AddListener(OnCreateRoomClicked);
            if (closeButton) closeButton.onClick.AddListener(() => gameObject.SetActive(false));
            if (searchRoomButton) searchRoomButton.onClick.AddListener(OnSearchRoomClicked);
        }

        private void OnSearchRoomClicked()
        {
            roomPanel.SetActive(true);
        }

        public void OnCreateRoomClicked()
        {
            RoomSettings settings = new RoomSettings
            {
                DeckType = DeckType.Standard48
            };

            // 1. 获取局数 (Rounds)
            if (toggleRounds3 != null && toggleRounds3.isOn) settings.Rounds = 3;
            else if (toggleRounds9 != null && toggleRounds9.isOn) settings.Rounds = 9;
            else if (toggleRounds18 != null && toggleRounds18.isOn) settings.Rounds = 18;
            else if (toggleRounds36 != null && toggleRounds36.isOn) settings.Rounds = 36;
            else settings.Rounds = 9; // 默认值

            // 2. 获取玩法模式 (PlayMode)
            if (toggleModeMust != null && toggleModeMust.isOn) settings.PlayMode = PlayMode.MustPlay;
            else settings.PlayMode = PlayMode.OptionalPlay;

            // 3. 获取先手规则 (FirstTurn)
            if (toggleTurnHeart3 != null && toggleTurnHeart3.isOn) settings.FirstTurn = FirstTurnRule.Heart3;
            else if (toggleTurnWinner != null && toggleTurnWinner.isOn) settings.FirstTurn = FirstTurnRule.Winner;
            else if (toggleTurnRotate != null && toggleTurnRotate.isOn) settings.FirstTurn = FirstTurnRule.Rotate;
            else settings.FirstTurn = FirstTurnRule.Heart3;

            // 4. 获取炸弹分 (BombScore)
            if (toggleBomb5 != null && toggleBomb5.isOn) settings.BombScore = 5;
            else settings.BombScore = 10;

            // 5. 获取布尔值规则
            if (toggleShowHandCount) settings.ShowHandCount = toggleShowHandCount.isOn;
            if (togglePayForAll) settings.PayForAll = togglePayForAll.isOn;
            if (toggleThreeAsBomb) settings.ThreeAsBomb = toggleThreeAsBomb.isOn;
            if (toggleNoLoseOnSingle) settings.NoLoseOnSingle = toggleNoLoseOnSingle.isOn;
            if (toggleRobPass) settings.RobPass = toggleRobPass.isOn;

            Debug.Log($"创建房间: {settings.Rounds}局, {settings.PlayMode}, {settings.FirstTurn}, 抢关:{settings.RobPass}");

            // 1. 获取 NetworkManager 的单例 (需要强转为你的子类)
            if (NetworkManager.singleton is RunFastNetworkManager manager)
            {
                // 2. 将设置“寄存”到 Manager 身上
                manager.PendingRoomSettings = settings;
                
                // 3. 启动主机 (Mirror 会自动检测并切换到 Online Scene)
                manager.StartHost();
                
                networkDiscovery.AdvertiseServer();
                
                // 4. 关闭当前 UI (因为马上要换场景了，关不关其实无所谓，但为了整洁)
                gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("场景中找不到 RunFastNetworkManager，请检查 NetworkManager 物体！");
            }
        }
    }
}