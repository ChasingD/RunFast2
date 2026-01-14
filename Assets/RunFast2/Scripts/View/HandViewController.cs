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

        [Header("Timer UI")]
        public TMP_Text TimerText; // 显示倒计时数字

        [Header("Rob UI")]
        public GameObject RobPanel;
        public Button RobButton;
        public Button NoRobButton;

        [Header("Result UI")]
        public GameObject RoundResultPanel; // 单局结算面板
        public TMP_Text RoundResultText;        // 显示结算信息
        public GameObject GameResultPanel;  // 总结算面板
        public TMP_Text GameResultText;         // 显示总结算信息
        public Button BackToLobbyButton;    // 返回大厅按钮

        [Header("Runtime State")]
        public List<CardView> CurrentCardViews = new List<CardView>();
        private CardPlayer _localPlayer;

        private void Start()
        {
            // Initial State
            if (ActionPanel) ActionPanel.SetActive(false);
            if (RobPanel) RobPanel.SetActive(false);
            if (RoundResultPanel) RoundResultPanel.SetActive(false);
            if (GameResultPanel) GameResultPanel.SetActive(false);
            if (TimerText) TimerText.gameObject.SetActive(false);

            if (PlayButton) PlayButton.onClick.AddListener(OnPlayClicked);
            if (PassButton) PassButton.onClick.AddListener(OnPassClicked);
            if (RobButton) RobButton.onClick.AddListener(() => OnRobClicked(true));
            if (NoRobButton) NoRobButton.onClick.AddListener(() => OnRobClicked(false));
            if (BackToLobbyButton) BackToLobbyButton.onClick.AddListener(OnBackToLobbyClicked);

            // Subscribe to Static Events
            CardPlayer.OnPlayerInfoUpdated += OnPlayerUpdated;
            CardPlayer.OnShowRobUI += OnShowRobUI;
            CardPlayer.OnRoundFinished += OnRoundFinished;
            CardPlayer.OnGameFinished += OnGameFinished;
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
            PokerManager.OnTurnChangedEvent -= OnTurnChanged;
            PokerManager.OnStateChangedEvent -= OnStateChanged;

            // Unsubscribe Instance Events
            if (_localPlayer != null)
            {
                _localPlayer.OnHandReceived -= RefreshHand;
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

            // 只有在 Playing 状态且有人出牌时才显示倒计时
            if (PokerManager.Instance.CurrentState != GameState.Playing)
            {
                TimerText.gameObject.SetActive(false);
                return;
            }

            double endTime = PokerManager.Instance.TurnEndTime;
            double remaining = endTime - NetworkTime.time;

            if (remaining > 0)
            {
                TimerText.gameObject.SetActive(true);
                TimerText.text = Mathf.CeilToInt((float)remaining).ToString();
                
                // 变色提醒：最后5秒变红
                TimerText.color = remaining <= 5 ? Color.red : Color.white;
            }
            else
            {
                TimerText.gameObject.SetActive(false);
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
                _localPlayer.OnHandReceived -= RefreshHand;
            }

            _localPlayer = p;

            // Subscribe new
            if (_localPlayer != null)
            {
                _localPlayer.OnHandReceived += RefreshHand;
                RefreshHand();
                CheckTurnButtons();
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
            if (newState == GameState.Playing || newState == GameState.Robbing)
            {
                if (RoundResultPanel) RoundResultPanel.SetActive(false);
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
            if (RoundResultPanel)
            {
                RoundResultPanel.SetActive(true);
                if (RoundResultText)
                {
                    string info = $"第 {result.RoundIndex} 局结算:\n";
                    foreach (var p in result.PlayerResults)
                    {
                        string role = p.IsRobber ? "[抢关]" : "";
                        string winStr = p.IsWinner ? "(赢)" : "";
                        info += $"座位{p.SeatIndex} {role}{winStr}: {p.ScoreChange}分 (剩{p.RemainingCardCount}张)\n";
                    }
                    RoundResultText.text = info;
                }
            }
            
            // 播放音效
            bool amIWinner = result.PlayerResults.Any(p => p.SeatIndex == _localPlayer.SeatIndex && p.IsWinner);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(amIWinner ? AudioManager.Instance.SFX_Win : AudioManager.Instance.SFX_Lose);
            }
        }

        void OnGameFinished(GameTotalResult result)
        {
            if (RoundResultPanel) RoundResultPanel.SetActive(false); // 隐藏单局结算
            if (GameResultPanel)
            {
                GameResultPanel.SetActive(true);
                if (GameResultText)
                {
                    string info = "游戏结束 - 总成绩:\n";
                    foreach (var score in result.TotalScores)
                    {
                        info += $"座位 {score.SeatIndex}: 总分 {score.Score}\n";
                    }
                    GameResultText.text = info;
                }
            }
        }

        void RefreshHand()
        {
            if (_localPlayer == null) return;

            // 使用 UniTask 启动异步方法
            AnimateDealCardsAsync().Forget();
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

            // 4. 【关键日志】方便排查
            // Debug.Log($"[CheckTurn] Index:{currentTurnIndex}, PlayersFound:{seatedPlayers.Count}, MySeat:{mySeatIndex}");

            // 5. 数据未同步完成时的容错处理
            if (currentTurnIndex >= seatedPlayers.Count)
            {
                // 此时说明还没收到所有玩家的数据，暂时隐藏面板，等待 OnPlayerUpdated 再次触发
                // 不要报错，这是正常的网络延迟现象
                if (ActionPanel) ActionPanel.SetActive(false);
                return;
            }

            // 1. 判断是否轮到我
            CardPlayer currentPlayer = seatedPlayers[currentTurnIndex];
            bool isMyTurn = (currentPlayer.SeatIndex == _localPlayer.SeatIndex);
    
            if (ActionPanel) ActionPanel.SetActive(isMyTurn);

            // 2. 核心修改："有出必出" 逻辑控制 Pass 按钮
            if (isMyTurn && PassButton != null)
            {
                // 默认可以过
                bool canPass = true;

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
                        bool hasMove = PokerRules.HasHandToBeat(_localPlayer.MyHand, PokerManager.Instance.LastHand);
                
                        // 如果有牌能管，就不能过 (canPass = false)
                        // 如果没牌能管，才可以过 (canPass = true)
                        if (hasMove)
                        {
                            canPass = false; 
                            // 这里可以加个UI提示，比如把Pass按钮变灰，文字改成"必须出牌"
                        }
                    }
                }

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