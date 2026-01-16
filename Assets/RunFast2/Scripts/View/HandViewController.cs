using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using RunFast2.Scripts.Network;
using System.Linq;
using RunFast2.Scripts.Logic;
using RunFast2.Scripts.Model;
using TMPro;
using RunFast2.Scripts.Manager;
using Cysharp.Threading.Tasks; // 引用 UniTask
using Ricimi; // 引用 Ricimi

namespace RunFast2.Scripts.View
{
    public class HandViewController : MonoBehaviour
    {
        [Header("UI References")]
        public Transform HandContainer;    // Parent for CardViews (HorizontalLayoutGroup)
        public GameObject CardViewPrefab;  // Prefab with CardView script

        public Button PlayButton;
        public Button PassButton;
        public GameObject ActionPanel;     // Panel containing Play/Pass buttons
        public TMP_Text MessageText;       // 新增：提示文本 (如"没有牌能大过上家")

        [Header("Timer UI")]
        public TMP_Text TimerText; // 显示出牌倒计时数字
        public TMP_Text RobTimerText; // 显示抢关倒计时数字

        [Header("Rob UI")]
        public GameObject RobPanel;
        public Button RobButton;
        public Button NoRobButton;
        public Slider AutoPlaySlider; // 修改：托管开关 (Slider)

        [Header("Result UI Prefabs")]
        public GameObject RoundResultPrefab; // 单局结算 Prefab (已废弃，保留兼容)
        public GameObject GameResultPrefab;  // 总结算 Prefab
        public GameObject ResultItemPrefab;  // 结算条目 Prefab (如果需要动态生成)

        [Header("Runtime State")]
        public List<CardView> CurrentCardViews = new List<CardView>();
        private CardPlayer _localPlayer;
        private PopupOpener _popupOpener; // 用于打开弹窗

        private void Awake()
        {
            // 确保有 PopupOpener 组件，或者动态添加
            _popupOpener = GetComponent<PopupOpener>();
            if (_popupOpener == null) _popupOpener = gameObject.AddComponent<PopupOpener>();
        }

        private void Start()
        {
            // Initial State
            if (ActionPanel) ActionPanel.SetActive(false);
            if (RobPanel) RobPanel.SetActive(false);
            if (TimerText) TimerText.gameObject.SetActive(false);
            if (RobTimerText) RobTimerText.gameObject.SetActive(false);
            if (MessageText) MessageText.gameObject.SetActive(false);
            if (AutoPlaySlider) AutoPlaySlider.gameObject.SetActive(false); // 默认隐藏

            if (PlayButton) PlayButton.onClick.AddListener(OnPlayClicked);
            if (PassButton) PassButton.onClick.AddListener(OnPassClicked);
            if (RobButton) RobButton.onClick.AddListener(() => OnRobClicked(true));
            if (NoRobButton) NoRobButton.onClick.AddListener(() => OnRobClicked(false));
            if (AutoPlaySlider) AutoPlaySlider.onValueChanged.AddListener(OnAutoPlayToggled);
            
            // Subscribe to Static Events
            CardPlayer.OnPlayerInfoUpdated += OnPlayerUpdated;
            CardPlayer.OnShowRobUI += OnShowRobUI;
            CardPlayer.OnRoundFinished += OnRoundFinished;
            CardPlayer.OnGameFinished += OnGameFinished;
            CardPlayer.OnAutoPlayStateChanged += OnAutoPlayStateChanged; // 订阅托管状态变化
            PokerManager.OnTurnChangedEvent += OnTurnChanged;
            PokerManager.OnStateChangedEvent += OnStateChanged;

            // Try to find local player if already exists
            FindLocalPlayer();
        }

        private void OnDestroy()
        {
            // Unsubscribe Static Events
            CardPlayer.OnPlayerInfoUpdated -= OnPlayerUpdated;
            CardPlayer.OnShowRobUI -= OnShowRobUI;
            CardPlayer.OnRoundFinished -= OnRoundFinished;
            CardPlayer.OnGameFinished -= OnGameFinished;
            CardPlayer.OnAutoPlayStateChanged -= OnAutoPlayStateChanged;
            PokerManager.OnTurnChangedEvent -= OnTurnChanged;
            PokerManager.OnStateChangedEvent -= OnStateChanged;

            // Unsubscribe Instance Events
            if (_localPlayer != null)
            {
                _localPlayer.OnInitialHandReceived -= PlayDealAnimation;
                _localPlayer.OnHandUpdated -= RefreshHand;
            }
        }

        private void Update()
        {
            // Polling fallback to find local player if not set
            if (_localPlayer == null) FindLocalPlayer();

            UpdateTimer();
        }

        void UpdateTimer()
        {
            if (PokerManager.Instance == null || TimerText == null) return;

            double endTime = PokerManager.Instance.TurnEndTime;
            double remaining = endTime - NetworkTime.time;
            string timeStr = remaining > 0 ? Mathf.CeilToInt((float)remaining).ToString() : "0";
            Color timeColor = remaining <= 5 ? Color.red : Color.white;

            // 1. 出牌倒计时
            if (PokerManager.Instance.CurrentState == GameState.Playing)
            {
                if (TimerText)
                {
                    // 只有轮到自己出牌时才显示倒计时 (或者你想一直显示当前回合者的倒计时也可以)
                    // 这里假设只显示自己的
                    bool isMyTurn = _localPlayer != null && PokerManager.Instance.CurrentPlayerIndex != -1 && 
                                    _localPlayer.SeatIndex == FindObjectsOfType<CardPlayer>()
                                        .Where(p => p.SeatIndex != -1)
                                        .OrderBy(p => p.SeatIndex)
                                        .ElementAtOrDefault(PokerManager.Instance.CurrentPlayerIndex)?.SeatIndex;

                    if (isMyTurn && remaining > 0)
                    {
                        TimerText.gameObject.SetActive(true);
                        TimerText.text = timeStr;
                        TimerText.color = timeColor;
                    }
                    else
                    {
                        TimerText.gameObject.SetActive(false);
                    }
                }
                if (RobTimerText) RobTimerText.gameObject.SetActive(false);
            }
            // 2. 抢关倒计时
            else if (PokerManager.Instance.CurrentState == GameState.Robbing)
            {
                if (RobTimerText)
                {
                    if (remaining > 0)
                    {
                        RobTimerText.gameObject.SetActive(true);
                        RobTimerText.text = timeStr;
                        RobTimerText.color = timeColor;
                    }
                    else
                    {
                        RobTimerText.gameObject.SetActive(false);
                    }
                }
                if (TimerText) TimerText.gameObject.SetActive(false);
            }
            else
            {
                if (TimerText) TimerText.gameObject.SetActive(false);
                if (RobTimerText) RobTimerText.gameObject.SetActive(false);
            }
        }

        void FindLocalPlayer()
        {
            var players = FindObjectsOfType<CardPlayer>();
            foreach(var p in players)
            {
                if (p.isLocalPlayer)
                {
                    SetLocalPlayer(p);
                    break;
                }
            }
        }

        void SetLocalPlayer(CardPlayer p)
        {
            // 即便是同一个玩家对象，也可能因为 SeatIndex 变化了而需要刷新按钮状态
            if (_localPlayer == p) 
            {
                CheckTurnButtons(); // <--- 新增这行
                return;
            }

            // Unsubscribe old
            if (_localPlayer != null)
            {
                _localPlayer.OnInitialHandReceived -= PlayDealAnimation;
                _localPlayer.OnHandUpdated -= RefreshHand;
            }

            _localPlayer = p;

            // Subscribe new
            if (_localPlayer != null)
            {
                _localPlayer.OnInitialHandReceived += PlayDealAnimation;
                _localPlayer.OnHandUpdated += RefreshHand;
                RefreshHand(); // 初始刷新
                CheckTurnButtons();
                
                // 初始化托管 Slider 状态
                if (AutoPlaySlider) AutoPlaySlider.value = _localPlayer.IsAutoPlay ? 1 : 0;
            }
        }

        // --- Event Handlers ---

        void OnPlayerUpdated(CardPlayer player)
        {
            // 1. 如果是本地玩家更新，依然需要重置引用
            if (player.isLocalPlayer)
            {
                SetLocalPlayer(player);
            }
    
            // 2. 【核心修复】无论是谁更新了（比如对手入座数据终于同步过来了），
            // 都会影响 seatedPlayers 列表的长度和顺序，所以必须重新检查回合！
            CheckTurnButtons();
        }

        void OnAutoPlayStateChanged(CardPlayer player)
        {
            if (player.isLocalPlayer && AutoPlaySlider != null)
            {
                // 避免循环触发
                float targetValue = player.IsAutoPlay ? 1 : 0;
                if (Mathf.Abs(AutoPlaySlider.value - targetValue) > 0.1f)
                {
                    AutoPlaySlider.SetValueWithoutNotify(targetValue);
                }
            }
        }

        void OnTurnChanged(int turnIndex)
        {
            Debug.Log($"[UI] 收到回合切换通知: {turnIndex}");
            CheckTurnButtons();
        }

        void OnStateChanged(GameState newState)
        {
            Debug.Log($"[UI] 收到状态切换通知: {newState}");
            CheckTurnButtons();
            
            // 如果进入新的一局 (Playing/Robbing)，隐藏结算面板
            // 注意：现在结算面板是 Popup，它会自动管理关闭，或者我们需要手动关闭
            // 这里假设 Popup 会在点击继续后关闭，或者新局开始时自动关闭
            // 如果需要自动关闭，我们需要持有 Popup 的引用，但因为是动态生成的，比较麻烦
            // 简单做法：让 RoundResultPanel 监听 GameState 变化并自动关闭

            // 控制 AutoPlaySlider 的显隐
            if (AutoPlaySlider != null)
            {
                bool show = newState == GameState.Playing || newState == GameState.Robbing;
                AutoPlaySlider.gameObject.SetActive(show);
            }
        }

        void OnShowRobUI(bool show)
        {
            if (RobPanel) RobPanel.SetActive(show);
            if (ActionPanel && show) ActionPanel.SetActive(false); // Hide play buttons when robbing
            
            if (!show) CheckTurnButtons(); // Refresh buttons when Rob UI hides
        }

        void OnRoundFinished(RoundResult result)
        {
            // 移除单局结算弹窗显示逻辑
            // 播放音效
            bool amIWinner = result.PlayerResults.Any(p => p.SeatIndex == _localPlayer.SeatIndex && p.IsWinner);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(amIWinner ? AudioManager.Instance.SFX_Win : AudioManager.Instance.SFX_Lose);
            }
        }

        void OnGameFinished(GameTotalResult result)
        {
            if (GameResultPrefab != null)
            {
                var canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    var popup = Instantiate(GameResultPrefab, canvas.transform);
                    // 确保 Popup 居中
                    var rect = popup.GetComponent<RectTransform>();
                    if (rect) rect.anchoredPosition = Vector2.zero;

                    var panel = popup.GetComponent<GameResultPanel>();
                    if (panel != null)
                    {
                        panel.Initialize(result);
                        panel.OnBackToLobbyClicked = OnBackToLobbyClicked;
                    }
                    
                    // 打开动画
                    var popupComp = popup.GetComponent<Popup>();
                    if (popupComp) popupComp.Open();
                }
            }
        }

        // 仅用于播放发牌动画
        void PlayDealAnimation()
        {
            if (_localPlayer == null) return;
            AnimateDealCardsAsync().Forget();
        }

        // 仅用于刷新手牌显示 (无动画)
        void RefreshHand()
        {
            if (_localPlayer == null) return;

            // Clear old views
            foreach (Transform child in HandContainer)
            {
                Destroy(child.gameObject);
            }
            CurrentCardViews.Clear();

            if (CardViewPrefab == null)
            {
                Debug.LogError("HandViewController: CardViewPrefab is missing!");
                return;
            }

            // 直接生成并显示
            foreach (var card in _localPlayer.MyHand)
            {
                GameObject go = Instantiate(CardViewPrefab, HandContainer);
                CardView view = go.GetComponent<CardView>();
                if (view != null)
                {
                    view.Initialize(card);
                    CurrentCardViews.Add(view);
                }
            }
        }

        async UniTaskVoid AnimateDealCardsAsync()
        {
            // Clear old views
            foreach (Transform child in HandContainer)
            {
                Destroy(child.gameObject);
            }
            CurrentCardViews.Clear();

            if (CardViewPrefab == null)
            {
                Debug.LogError("HandViewController: CardViewPrefab is missing!");
                return;
            }

            // 播放发牌音效
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Deal);

            // 逐个生成并移动
            foreach (var card in _localPlayer.MyHand)
            {
                GameObject go = Instantiate(CardViewPrefab, HandContainer);
                CardView view = go.GetComponent<CardView>();
                if (view != null)
                {
                    view.Initialize(card);
                    CurrentCardViews.Add(view);
                    
                    // 简单的动画：先隐藏，再显示
                    go.SetActive(false);
                    await UniTask.Delay(50); // 50ms
                    if (go != null) go.SetActive(true);
                }
            }
        }

        // 位于 HandViewController.cs 中

        void CheckTurnButtons()
        {
            // 1. 基础检查
            if (_localPlayer == null || PokerManager.Instance == null)
            {
                if (ActionPanel) ActionPanel.SetActive(false);
                return;
            }

            // If we are in robbing state, don't show play buttons
            if (PokerManager.Instance.CurrentState == GameState.Robbing)
            {
                if (ActionPanel) ActionPanel.SetActive(false);
                return;
            }

            int currentTurnIndex = PokerManager.Instance.CurrentPlayerIndex;
            int mySeatIndex = _localPlayer.SeatIndex;

            // 2. 无效状态处理
            if (currentTurnIndex == -1 || mySeatIndex == -1)
            {
                if (ActionPanel) ActionPanel.SetActive(false);
                return;
            }

            // 3. 重建列表
            var seatedPlayers = FindObjectsOfType<CardPlayer>()
                .Where(p => p.SeatIndex != -1)
                .OrderBy(p => p.SeatIndex)
                .ToList();

            // 5. 数据未同步完成时的容错处理
            if (currentTurnIndex >= seatedPlayers.Count)
            {
                if (ActionPanel) ActionPanel.SetActive(false);
                return;
            }

            // 1. 判断是否轮到我
            CardPlayer currentPlayer = seatedPlayers[currentTurnIndex];
            bool isMyTurn = (currentPlayer.SeatIndex == _localPlayer.SeatIndex);
    
            if (ActionPanel) ActionPanel.SetActive(isMyTurn);
            if (MessageText) MessageText.gameObject.SetActive(false); // 默认隐藏提示

            // 2. 核心修改："有出必出" 逻辑控制 Pass 按钮
            if (isMyTurn && PassButton != null)
            {
                // 默认可以过
                bool canPass = true;
                bool hasMove = true; // 默认假设有牌能管

                // A. 如果我是首发 (LastHand 为空，或者是上一轮我自己出的)，那肯定不能过，必须出牌
                bool isRoundLeader = (PokerManager.Instance.LastPlayerSeatIndex == _localPlayer.SeatIndex)
                                     || (PokerManager.Instance.LastHand == null);
        
                if (isRoundLeader)
                {
                    canPass = false;
                }
                else
                {
                    // B. 如果不是首发，检查"有出必出"规则
                    // 获取当前房间设置 (假设 RoomSettings 有 PlayMode 字段，0=有出必出)
                    bool mustPlayMode = true; // 你可以从 PokerManager.Instance.CurrentSettings.PlayMode 获取
            
                    if (mustPlayMode)
                    {
                        // 使用刚刚写的 PokerRules 方法检测
                        hasMove = PokerRules.HasHandToBeat(_localPlayer.MyHand, PokerManager.Instance.LastHand);
                
                        // 如果有牌能管，就不能过 (canPass = false)
                        // 如果没牌能管，才可以过 (canPass = true)
                        if (hasMove)
                        {
                            canPass = false; 
                        }
                        else
                        {
                            // 没牌能管，显示提示
                            if (MessageText)
                            {
                                MessageText.gameObject.SetActive(true);
                                MessageText.text = "没有牌能大过上家";
                            }
                        }
                    }
                }

                // 无论如何，PassButton 始终显示，只是 interactable 状态不同
                // 或者：如果没牌能管，PassButton 必须启用，让玩家点击过牌
                // 如果有牌能管且是必出模式，PassButton 禁用
                
                PassButton.interactable = canPass;
            }
        }

        // --- Button Actions ---

        void OnPlayClicked()
        {
            if (_localPlayer == null) return;

            // 1. 收集选中的牌
            List<Card> selectedCards = new List<Card>();
            foreach (var view in CurrentCardViews)
            {
                if (view.IsSelected)
                {
                    selectedCards.Add(view.CardData);
                }
            }

            // 基础非空检查
            if (selectedCards.Count == 0)
            {
                Debug.Log("请先选择要出的牌。");
                // 建议：这里调用 DialogManager 显示 "请选牌"
                return;
            }

            // --- 【新增逻辑：本地合法性校验】Start ---

            // 2. 验证牌型是否合法 (例如：是不是顺子、对子、三带一等)
            PokerHand myHand = PokerRules.AnalyzeHand(selectedCards);

            if (myHand.Type == HandType.Invalid)
            {
                Debug.LogWarning("出牌不合理：无效的牌型！");
                // 建议：UI提示 "牌型无效"
                return; 
            }

            // 3. 验证是否管得住上家 (比大小)
            if (PokerManager.Instance != null)
            {
                // 判断是否是"必须管牌"的情况
                // 如果 LastHand 不为空，且 上一手牌不是我自己出的 (即我不是获得球权的人)
                bool mustBeatOthers = (PokerManager.Instance.LastHand != null) && 
                                      (PokerManager.Instance.LastPlayerSeatIndex != _localPlayer.SeatIndex);

                if (mustBeatOthers)
                {
                    // 检查能否压制
                    // 注意：PokerManager.Instance.LastHand 需要确保是 PokerHand 类型
                    // 如果你的 LastHand 是可空的，需要处理 .Value
                    bool canBeat = PokerRules.CanBeat(PokerManager.Instance.LastHand, myHand);

                    if (!canBeat)
                    {
                        Debug.LogWarning("出牌不合理：你的牌管不上上家！");
                        // 建议：UI提示 "打不过" 或 "牌型不符"
                        return;
                    }
                }
            }
            // --- 【新增逻辑：本地合法性校验】End ---


            // 4. 全部校验通过，才发送给服务器
            _localPlayer.CmdPlayCard(selectedCards.ToArray());
            
            // 播放出牌音效
            if (AudioManager.Instance != null) AudioManager.Instance.PlayCardSound(myHand.Type);
        }

        void OnPassClicked()
        {
            if (_localPlayer == null) return;
            _localPlayer.CmdPass();
            
            // 播放不要音效
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Pass);
        }

        void OnRobClicked(bool wantToRob)
        {
            if (_localPlayer == null) return;
            _localPlayer.CmdRobPass(wantToRob);
            if (RobPanel) RobPanel.SetActive(false);
            
            // 播放按钮音效
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.SFX_Button);
        }

        void OnAutoPlayToggled(float value)
        {
            if (_localPlayer == null) return;
            _localPlayer.CmdToggleAutoPlay(value > 0.5f); // 阈值判断
        }

        void OnBackToLobbyClicked()
        {
            // 简单的断开连接返回大厅
            if (NetworkManager.singleton != null)
            {
                NetworkManager.singleton.StopClient();
                if (NetworkServer.active) NetworkManager.singleton.StopHost();
            }
        }
    }
}