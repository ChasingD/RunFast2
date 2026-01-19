using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using RunFast2.Scripts.Network;
using RunFast2.Scripts.Model;
using RunFast2.Scripts.Logic;
using PlayMode = RunFast2.Scripts.Model.PlayMode;
using System.Collections;

public enum GameState
{
    Waiting,
    Robbing,
    Playing,
    RoundFinished,
    GameFinished
}

public class PokerManager : NetworkBehaviour
{
    public static PokerManager Instance;

    private List<Card> _deck = new List<Card>();

    [SyncVar(hook = nameof(OnTurnChanged))]
    public int CurrentPlayerIndex = -1; // Index in the seatedPlayers array

    [SyncVar(hook = nameof(OnStateChanged))]
    public GameState CurrentState = GameState.Waiting;

    [SyncVar]
    public int RobberSeatIndex = -1;

    [SyncVar]
    public int CurrentRoundCount = 0; // 当前第几局 (从1开始)

    [SyncVar]
    public double TurnEndTime; // 倒计时结束时间 (NetworkTime.time)

    public float TurnDuration = 15.0f; // 每回合秒数
    public float RobDuration = 5.0f; // 抢关秒数

    private HashSet<int> _robResponses = new HashSet<int>();

    // Current Round State
    public PokerHand LastHand = null;
    public int LastPlayerSeatIndex = -1; // The seat index of the player who played LastHand

    public RoomSettings CurrentSettings = new RoomSettings();
    
    // Game History
    public GameTotalResult GameResult = new GameTotalResult();

    // Events for UI (Local)
    public static event System.Action<int> OnTurnChangedEvent;
    public static event System.Action<GameState> OnStateChangedEvent;
    public static event System.Action<RoundResult> OnRoundResultEvent; // 客户端收到单局结算
    public static event System.Action<GameTotalResult> OnGameResultEvent; // 客户端收到总结算
    
    [Header("Bot Settings")]
    public GameObject CardPlayerPrefab; // 确保在 Inspector 中赋值 CardPlayer 的预制体

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (isServer)
        {
            CheckTurnTimeout();
        }
    }

    [Server]
    void CheckTurnTimeout()
    {
        if (CurrentState == GameState.Playing)
        {
            if (CurrentPlayerIndex == -1 || NetworkTime.time < TurnEndTime) return;
            HandlePlayTimeout();
        }
        else if (CurrentState == GameState.Robbing)
        {
            if (NetworkTime.time < TurnEndTime) return;
            HandleRobTimeout();
        }
    }

    [Server]
    void HandlePlayTimeout()
    {
        // 超时处理
        var seatedPlayers = GetSeatedPlayers();
        if (CurrentPlayerIndex >= seatedPlayers.Length) return;

        CardPlayer currentPlayer = seatedPlayers[CurrentPlayerIndex];
        Debug.Log($"[Timeout] Player {currentPlayer.SeatIndex} timed out.");

        // 尝试过牌
        bool canPass = true;
        
        // 1. 如果是首发，不能过
        if (LastPlayerSeatIndex == currentPlayer.SeatIndex || LastHand == null)
        {
            Debug.Log($"[Timeout] Player is Round Leader. LastHand: {(LastHand == null ? "null" : LastHand.ToString())}, LastSeat: {LastPlayerSeatIndex}");
            canPass = false;
        }
        // 2. 如果是有出必出且有牌能管，不能过
        else if (CurrentSettings.PlayMode == PlayMode.MustPlay)
        {
            bool hasMove = PokerRules.HasHandToBeat(currentPlayer.ServerHand, LastHand);
            if (hasMove)
            {
                Debug.Log("[Timeout] MustPlay mode & Has move. Cannot pass.");
                canPass = false;
            }
            else
            {
                Debug.Log("[Timeout] MustPlay mode & No move. Can pass.");
            }
        }

        if (canPass)
        {
            Debug.Log("[Timeout] Executing Pass.");
            OnPlayerPass(currentPlayer);
        }
        else
        {
            Debug.Log("[Timeout] Executing Auto Play.");
            // 必须出牌但超时了 -> 自动出最小的一张牌 (或者最小的能管上的牌)
            // 调用 PokerRules.GetSmallestBeatHand
            
            List<Card> cardsToPlay = null;

            if (LastHand == null || LastPlayerSeatIndex == currentPlayer.SeatIndex)
            {
                // 首发：出最小的一张
                if (currentPlayer.ServerHand.Count > 0)
                {
                    // 简单排序找最小
                    var sortedHand = currentPlayer.ServerHand.OrderBy(c => c.GetLogicWeight()).ToList();
                    cardsToPlay = new List<Card> { sortedHand[0] };
                    Debug.Log($"[Timeout] Auto Play First: {cardsToPlay[0]}");
                }
            }
            else
            {
                // 管牌：找最小能管上的
                Debug.Log($"[Timeout] Auto Play Beat: LastHand {LastHand}, MyHand Count {currentPlayer.ServerHand.Count}");
                cardsToPlay = PokerRules.GetSmallestBeatHand(currentPlayer.ServerHand, LastHand);
                
                if (cardsToPlay == null)
                {
                    // 理论上 HasHandToBeat 检查过了，不应该为空，除非逻辑不一致
                    // 如果真的为空，强制过牌
                    Debug.LogWarning($"[Timeout] Player {currentPlayer.SeatIndex} timed out and must play, but GetSmallestBeatHand returned null. Forcing Pass.");
                    currentPlayer.RpcOnPlayerPassed(currentPlayer.SeatIndex);
                    NextTurn(seatedPlayers);
                    return;
                }
                else
                {
                    Debug.Log($"[Timeout] Auto Play Found: {string.Join(",", cardsToPlay)}");
                }
            }

            if (cardsToPlay != null)
            {
                OnPlayerPlayCard(currentPlayer, cardsToPlay.ToArray());
            }
            else
            {
                // 理论上不应该到这，除非手牌为空但没结算
                Debug.LogError("[Timeout] Timeout handling failed. No cards to play.");
                NextTurn(seatedPlayers);
            }
        }
    }
    
    // 新增：供 CardPlayer 调用的立即自动出牌方法
    [Server]
    public void CheckAutoPlay(CardPlayer player)
    {
        if (CurrentState != GameState.Playing) return;
        
        var seatedPlayers = GetSeatedPlayers();
        if (CurrentPlayerIndex >= seatedPlayers.Length) return;
        
        // 只有轮到该玩家时才处理
        if (seatedPlayers[CurrentPlayerIndex] != player) return;

        // 如果是机器人 OR 玩家开启了托管，都执行自动出牌
        if (player.IsBot || player.IsAutoPlay)
        {
            StartCoroutine(AutoPlayCoroutine(player));
        }
    }

    [Server]
    IEnumerator AutoPlayCoroutine(CardPlayer player)
    {
        // 机器人思考时间：1~2秒随机，显得更真实
        float delay = UnityEngine.Random.Range(1.0f, 2.0f);
        yield return new WaitForSeconds(delay);

        // 再次检查状态，防止状态已变
        if (CurrentState != GameState.Playing) yield break;
        var seatedPlayers = GetSeatedPlayers();
        if (CurrentPlayerIndex >= seatedPlayers.Length) yield break;
        if (seatedPlayers[CurrentPlayerIndex] != player) yield break;
        
        // 再次确认标记 (防止玩家中途取消托管，但机器人 IsBot 永远为 true)
        if (!player.IsBot && !player.IsAutoPlay) yield break;

        // === AI 核心逻辑 ===
        List<Card> cardsToPlay = null;

        // 判断是首发还是跟牌
        PokerHand handToBeat = null;
        if (LastHand != null && LastPlayerSeatIndex != player.SeatIndex)
        {
            handToBeat = LastHand;
        }

        // 调用 AI 获取出牌
        cardsToPlay = PokerAI.GetBestMove(player.ServerHand, handToBeat);

        if (cardsToPlay != null && cardsToPlay.Count > 0)
        {
            Debug.Log($"[AI] Player {player.SeatIndex} plays: {cardsToPlay.Count} cards");
            OnPlayerPlayCard(player, cardsToPlay.ToArray());
        }
        else
        {
            Debug.Log($"[AI] Player {player.SeatIndex} passes");
            OnPlayerPass(player);
        }
    }

    [Server]
    void HandleRobTimeout()
    {
        Debug.Log("抢关超时，所有未响应玩家视为不抢。");
        var seatedPlayers = GetSeatedPlayers();
        foreach (var player in seatedPlayers)
        {
            if (!_robResponses.Contains(player.SeatIndex))
            {
                OnPlayerRob(player, false);
            }
        }
    }

    [Server]
    public void InitializeGame(RoomSettings settings)
    {
        CurrentSettings = settings;
        CurrentRoundCount = 0;
        GameResult = new GameTotalResult 
        { 
            RoundHistory = new List<RoundResult>(),
            PlayerStats = new List<PlayerTotalStats>() 
        };
        StartNewRound();
    }

    [Server]
    public void StartNewRound()
    {
        Debug.Log($"StartNewRound: Current {CurrentRoundCount}, Max {CurrentSettings.Rounds}");
        CurrentRoundCount++;
        if (CurrentRoundCount > CurrentSettings.Rounds)
        {
            Debug.Log("Max rounds reached. Finishing game.");
            FinishGame();
            return;
        }

        var allPlayers = FindObjectsOfType<CardPlayer>();
        var seatedPlayers = allPlayers
            .Where(p => p.SeatIndex != -1)
            .OrderBy(p => p.SeatIndex)
            .ToArray();

        if (seatedPlayers.Length < 2) 
        {
            Debug.LogWarning("Not enough players seated to start.");
            return;
        }

        // Reset Round State
        LastHand = null;
        LastPlayerSeatIndex = -1;
        RobberSeatIndex = -1;
        _robResponses.Clear();

        InitDeck();
        ShuffleDeck();
        DealCards(seatedPlayers);

        if (CurrentSettings.RobPass)
        {
            CurrentState = GameState.Robbing;
            CurrentPlayerIndex = -1;
            TurnEndTime = NetworkTime.time + RobDuration; // 设置抢关倒计时
            foreach (var p in seatedPlayers)
            {
                p.RpcShowRobUI();
            }
            
            // 遍历所有玩家，如果是机器人，延迟后自动决策
            foreach (var p in seatedPlayers)
            {
                if (p.IsBot)
                {
                    StartCoroutine(BotRobCoroutine(p));
                }
            }
        }
        else
        {
            CurrentState = GameState.Playing;
            DetermineStartingPlayer(seatedPlayers);
        }
    }
    
    [Server]
    IEnumerator BotRobCoroutine(CardPlayer bot)
    {
        yield return new WaitForSeconds(2.0f);
        // 简单策略：有炸弹或者大牌多就抢，这里暂时随机或者不抢
        bool wantRob = false; 
        OnPlayerRob(bot, wantRob);
    }

    [Server]
    public void OnPlayerRob(CardPlayer player, bool wantToRob)
    {
        if (CurrentState != GameState.Robbing) return;
        if (_robResponses.Contains(player.SeatIndex)) return;

        _robResponses.Add(player.SeatIndex);

        if (wantToRob)
        {
            // Rob Success
            RobberSeatIndex = player.SeatIndex;
            CurrentState = GameState.Playing;
            CurrentPlayerIndex = player.SeatIndex;
            TurnEndTime = NetworkTime.time + TurnDuration; // Set Timer

            var seated = GetSeatedPlayers();
            foreach (var p in seated)
            {
                p.RpcHideRobUI();
                p.RpcOnRobResult(RobberSeatIndex);
            }
            
            // 检查是否需要自动出牌 (如果抢关者开启了托管)
            if (player.IsAutoPlay || player.IsBot)
            {
                CheckAutoPlay(player);
            }
        }
        else
        {
            var seated = GetSeatedPlayers();
            if (_robResponses.Count >= seated.Length)
            {
                // All passed
                RobberSeatIndex = -1;
                CurrentState = GameState.Playing;
                DetermineStartingPlayer(seated);

                foreach (var p in seated)
                {
                    p.RpcHideRobUI();
                    p.RpcOnRobResult(-1);
                }
            }
            else
            {
                // Just hide UI for this player
                player.RpcHideRobUI();
            }
        }
    }

    [Server]
    public void OnPlayerFirstTurn(CardPlayer player)
    {
        if (CurrentState != GameState.Robbing) return;
        if (_robResponses.Contains(player.SeatIndex)) return;

        // "我先出" 逻辑：
        // 视为抢关阶段结束，该玩家获得先手权，但 RobberSeatIndex 保持为 -1 (无抢关者)
        _robResponses.Add(player.SeatIndex);
        
        RobberSeatIndex = -1; 
        CurrentState = GameState.Playing;
        CurrentPlayerIndex = player.SeatIndex; // 指定该玩家先出
        TurnEndTime = NetworkTime.time + TurnDuration;

        var seated = GetSeatedPlayers();
        foreach (var p in seated)
        {
            p.RpcHideRobUI();
            p.RpcOnRobResult(-1); // 通知大家没人抢关
        }

        // 检查是否需要自动出牌
        if (player.IsAutoPlay || player.IsBot)
        {
            CheckAutoPlay(player);
        }
    }

    [Server]
    void InitDeck()
    {
        _deck.Clear();

        if (CurrentSettings.DeckType == DeckType.Standard48)
        {
            foreach (CardSuit suit in System.Enum.GetValues(typeof(CardSuit)))
            {
                foreach (CardRank rank in System.Enum.GetValues(typeof(CardRank)))
                {
                    if (rank == CardRank.Two && suit != CardSuit.Spade) continue;
                    if (rank == CardRank.Ace && suit == CardSuit.Spade) continue;
                    _deck.Add(new Card(suit, rank));
                }
            }
        }
        else
        {
            foreach (CardSuit suit in System.Enum.GetValues(typeof(CardSuit)))
            {
                foreach (CardRank rank in System.Enum.GetValues(typeof(CardRank)))
                {
                    _deck.Add(new Card(suit, rank));
                }
            }
        }
    }

    [Server]
    void ShuffleDeck()
    {
        System.Random rng = new System.Random();
        int n = _deck.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Card value = _deck[k];
            _deck[k] = _deck[n];
            _deck[n] = value;
        }
    }

    [Server]
    void DealCards(CardPlayer[] players)
    {
        List<List<Card>> hands = new List<List<Card>>();
        for (int i = 0; i < players.Length; i++) hands.Add(new List<Card>());

        for (int i = 0; i < _deck.Count; i++)
        {
            int seatIndex = i % players.Length;
            hands[seatIndex].Add(_deck[i]);
        }

        for (int i = 0; i < players.Length; i++)
        {
            CardPlayer player = players[i];
            player.ServerHand.Clear();
            player.ServerHand.AddRange(hands[i]);
            player.RemainingCardCount = hands[i].Count; // 初始化牌数
            
            // 如果是机器人，不需要发送 TargetRpc (因为没有客户端连接)
            // 但为了代码统一，调用也没关系，Mirror 会处理
            if (!player.IsBot)
            {
                player.TargetRpcReceiveHand(player.connectionToClient, hands[i].ToArray());
            }
            else
            {
                // 机器人直接设置 MyHand (虽然服务器端 MyHand 没用，但保持一致性)
                player.MyHand.Clear();
                player.MyHand.AddRange(hands[i]);
            }
        }
    }

    [Server]
    void DetermineStartingPlayer(CardPlayer[] players)
    {
        int startingIndex = 0;

        // 如果不是第一局，且规则是赢家先出
        if (CurrentRoundCount > 1 && CurrentSettings.FirstTurn == FirstTurnRule.Winner)
        {
            // 找到上一局的赢家
            if (GameResult.RoundHistory.Count > 0)
            {
                var lastRound = GameResult.RoundHistory.Last();
                var winner = lastRound.PlayerResults.FirstOrDefault(r => r.IsWinner);
                // 找到对应的座位索引
                for(int i=0; i<players.Length; i++)
                {
                    if (players[i].SeatIndex == winner.SeatIndex)
                    {
                        startingIndex = i;
                        break;
                    }
                }
            }
        }
        else if (CurrentSettings.FirstTurn == FirstTurnRule.Heart3)
        {
            bool found = false;
            for (int i = 0; i < players.Length; i++)
            {
                foreach (var card in players[i].ServerHand)
                {
                    if (card.Suit == CardSuit.Heart && card.Rank == CardRank.Three)
                    {
                        startingIndex = i;
                        found = true;
                        break;
                    }
                }
                if (found) break;
            }
        }
        // Rotate rule logic can be added here if needed

        CurrentPlayerIndex = startingIndex;
        TurnEndTime = NetworkTime.time + TurnDuration; // Set Timer
        
        // 检查起始玩家是否托管
        if (players[startingIndex].IsAutoPlay || players[startingIndex].IsBot)
        {
            CheckAutoPlay(players[startingIndex]);
        }
    }

    // ==========================================
    // Game Loop Implementation
    // ==========================================

    [Server]
    public void OnPlayerPlayCard(CardPlayer player, Card[] cards)
    {
        var seatedPlayers = GetSeatedPlayers();
        if (CurrentPlayerIndex < 0 || CurrentPlayerIndex >= seatedPlayers.Length) return;

        CardPlayer currentPlayer = seatedPlayers[CurrentPlayerIndex];

        if (player != currentPlayer)
        {
            Debug.LogWarning($"Player {player.SeatIndex} tried to play out of turn.");
            return;
        }

        List<Card> cardList = new List<Card>(cards);
        if (!HasCards(player, cardList))
        {
            Debug.LogWarning($"Player {player.SeatIndex} does not have the cards.");
            return;
        }

        bool threeAsBomb = CurrentSettings.ThreeAsBomb;
        PokerHand hand = PokerRules.AnalyzeHand(cardList, threeAsBomb);

        if (hand.Type == HandType.Invalid)
        {
            Debug.LogWarning($"Player {player.SeatIndex} played invalid hand.");
            return;
        }

        bool isNewRound = (LastHand == null) || (LastPlayerSeatIndex == player.SeatIndex);

        if (!isNewRound)
        {
            if (!PokerRules.CanBeat(LastHand, hand))
            {
                Debug.LogWarning($"Player {player.SeatIndex} hand cannot beat last hand.");
                return;
            }
        }

        // --- Execute Play ---
        foreach (var c in cardList)
        {
            var serverCard = player.ServerHand.FirstOrDefault(sc => sc.Suit == c.Suit && sc.Rank == c.Rank);
            player.ServerHand.Remove(serverCard);
        }
        player.RemainingCardCount = player.ServerHand.Count; // 更新牌数

        int previousLastPlayer = LastPlayerSeatIndex;

        LastHand = hand;
        LastPlayerSeatIndex = player.SeatIndex;

        // Correct RPC Call: Invoke ClientRpc on the NetworkBehaviour
        player.RpcOnPlayerPlayed(player.SeatIndex, cards, (int)hand.Type);

        // --- 炸弹即时结算 ---
        if (hand.Type == HandType.Bomb)
        {
            int bombScore = CurrentSettings.BombScore;
            // 扣除其他两家分数，加给当前玩家
            // 更新 GameResult.PlayerStats
            UpdateBombScore(player.SeatIndex, bombScore, seatedPlayers);

            // --- 新增：炸弹触发地震特效 (延迟1秒) ---
            StartCoroutine(TriggerBombEarthquake(player, seatedPlayers));
        }

        // --- Rob Pass Failure Check ---
        if (CurrentSettings.RobPass && RobberSeatIndex != -1)
        {
            // If the previous hand was played by the Robber, and the current player is NOT the robber
            // It means the Robber is being beaten.
            if (previousLastPlayer == RobberSeatIndex && player.SeatIndex != RobberSeatIndex)
            {
                Debug.Log($"[RobPass] Robber {RobberSeatIndex} beaten by {player.SeatIndex}. Game Over.");
                // 抢关失败，当前出牌者获胜
                CalculateRoundScore(player.SeatIndex, true); 
                return;
            }
        }

        if (player.ServerHand.Count == 0)
        {
            // 正常获胜
            CalculateRoundScore(player.SeatIndex, false);
            return;
        }

        NextTurn(seatedPlayers);
    }

    [Server]
    IEnumerator TriggerBombEarthquake(CardPlayer sourcePlayer, CardPlayer[] seatedPlayers)
    {
        yield return new WaitForSeconds(1.0f);
        
        // 再次检查状态，防止游戏已结束
        if (CurrentState != GameState.Playing && CurrentState != GameState.RoundFinished) yield break;

        foreach (var p in seatedPlayers)
        {
            if (p.SeatIndex != sourcePlayer.SeatIndex)
            {
                // 模拟使用道具：源头是出牌者，目标是其他玩家，道具类型是地震
                sourcePlayer.RpcOnItemUsed(sourcePlayer.SeatIndex, p.SeatIndex, (int)ItemType.Earthquake);
            }
        }
    }

    [Server]
    void UpdateBombScore(int bombPlayerSeat, int scorePerPlayer, CardPlayer[] seatedPlayers)
    {
        int totalGain = 0;
        foreach (var p in seatedPlayers)
        {
            if (p.SeatIndex != bombPlayerSeat)
            {
                UpdatePlayerTotalScore(p.SeatIndex, -scorePerPlayer);
                totalGain += scorePerPlayer;
            }
        }
        UpdatePlayerTotalScore(bombPlayerSeat, totalGain);
        
        // 增加炸弹计数
        var stats = GameResult.PlayerStats.FirstOrDefault(s => s.SeatIndex == bombPlayerSeat);
        if (!stats.Equals(default(PlayerTotalStats)))
        {
            int index = GameResult.PlayerStats.IndexOf(stats);
            var newStats = stats;
            newStats.BombCount++;
            GameResult.PlayerStats[index] = newStats;
        }

        // 同步分数给客户端
        foreach (var p in seatedPlayers)
        {
            p.RpcUpdateScores(GameResult.PlayerStats.ToArray());
        }
    }

    [Server]
    void UpdatePlayerTotalScore(int seatIndex, int delta)
    {
        var existingScore = GameResult.PlayerStats.FirstOrDefault(s => s.SeatIndex == seatIndex);
        if (existingScore.Equals(default(PlayerTotalStats)))
        {
            // 如果还没记录，先初始化
            // 注意：这里可能拿不到 PlayerName，如果还没结算过。
            // 可以在 StartGame 时初始化所有人的 Stats
            // 这里简单处理：如果找不到，就新建一个空的
             GameResult.PlayerStats.Add(new PlayerTotalStats 
             { 
                 SeatIndex = seatIndex, 
                 TotalScore = delta 
             });
        }
        else
        {
            int index = GameResult.PlayerStats.IndexOf(existingScore);
            var newStats = existingScore;
            newStats.TotalScore += delta;
            GameResult.PlayerStats[index] = newStats;
        }
    }

    [Server]
    public void OnPlayerPass(CardPlayer player)
    {
        var seatedPlayers = GetSeatedPlayers();
        if (CurrentPlayerIndex < 0 || CurrentPlayerIndex >= seatedPlayers.Length) return;

        CardPlayer currentPlayer = seatedPlayers[CurrentPlayerIndex];

        // 1. 校验是否轮到该玩家
        if (player != currentPlayer) return;

        // 2. 基础规则：如果是这一轮的首发者（上一手是自己出的，或者是新的一轮），绝对不能过
        if (LastPlayerSeatIndex == player.SeatIndex || LastHand == null)
        {
            Debug.LogWarning($"Player {player.SeatIndex} is round leader and cannot pass.");
            return;
        }

        // --- 【新增】有出必出 (MustPlay) 校验 ---
        if (CurrentSettings.PlayMode == PlayMode.MustPlay)
        {
            // 调用 PokerRules.HasHandToBeat 检查玩家手牌
            // 逻辑：如果玩家有牌能管住 LastHand，则不允许 Pass
            if (PokerRules.HasHandToBeat(player.ServerHand, LastHand))
            {
                Debug.LogWarning($"[MustPlay] Player {player.SeatIndex} 试图过牌，但他手牌里有能管上的组合。已拒绝。");
                return; 
            }
        }
        // ---------------------------------------

        // 3. 校验通过，允许过牌
        player.RpcOnPlayerPassed(player.SeatIndex);

        NextTurn(seatedPlayers);
    }

    [Server]
    void NextTurn(CardPlayer[] seatedPlayers)
    {
        int playerCount = seatedPlayers.Length;
        // 修改为逆时针：(CurrentPlayerIndex + 1) % playerCount
        CurrentPlayerIndex = (CurrentPlayerIndex + 1) % playerCount;

        CardPlayer nextPlayer = seatedPlayers[CurrentPlayerIndex];

        if (nextPlayer.SeatIndex == LastPlayerSeatIndex)
        {
             LastHand = null;
             Debug.Log($"Player {nextPlayer.SeatIndex} wins round. New round.");
        }

        TurnEndTime = NetworkTime.time + TurnDuration; // Set Timer
        Debug.Log($"Next Turn: Player {nextPlayer.SeatIndex}");
        
        // 检查下一位玩家是否托管
        if (nextPlayer.IsAutoPlay || nextPlayer.IsBot)
        {
            CheckAutoPlay(nextPlayer);
        }
    }

    [Server]
    void CalculateRoundScore(int winnerSeatIndex, bool isRobFailure)
    {
        CurrentState = GameState.RoundFinished;
        var seatedPlayers = GetSeatedPlayers();
        
        // 1. 计算基础分
        int totalScorePool = 0;
        Dictionary<int, int> scores = new Dictionary<int, int>();

        // 初始化
        foreach (var p in seatedPlayers) scores[p.SeatIndex] = 0;

        if (CurrentSettings.RobPass && RobberSeatIndex != -1)
        {
            // --- 抢关模式结算 ---
            if (isRobFailure)
            {
                int penalty = 0;
                foreach (var p in seatedPlayers)
                {
                    if (p.SeatIndex != RobberSeatIndex)
                    {
                        int score = 10; // 基础奖励
                        scores[p.SeatIndex] = score;
                        penalty += score;
                    }
                }
                scores[RobberSeatIndex] = -penalty;
            }
            else
            {
                foreach (var p in seatedPlayers)
                {
                    if (p.SeatIndex != winnerSeatIndex)
                    {
                        int cardCount = p.ServerHand.Count;
                        int score = cardCount * 2; 
                        scores[p.SeatIndex] = -score;
                        totalScorePool += score;
                    }
                }
                scores[winnerSeatIndex] = totalScorePool;
            }
        }
        else
        {
            // --- 普通模式结算 ---
            foreach (var p in seatedPlayers)
            {
                if (p.SeatIndex != winnerSeatIndex)
                {
                    int cardCount = p.ServerHand.Count;
                    int score = cardCount;
                    
                    if (cardCount >= 10) score *= 2; // 关门翻倍

                    scores[p.SeatIndex] = -score;
                    totalScorePool += score;
                }
            }
            scores[winnerSeatIndex] = totalScorePool;
        }

        // 2. 更新总分并记录历史
        RoundResult roundResult = new RoundResult
        {
            RoundIndex = CurrentRoundCount,
            PlayerResults = new List<PlayerRoundResult>()
        };

        foreach (var p in seatedPlayers)
        {
            bool isDoubleClose = p.ServerHand.Count >= 10; 
            bool isSingleClose = false; 

            var result = new PlayerRoundResult
            {
                SeatIndex = p.SeatIndex,
                PlayerName = p.PlayerName,
                ScoreChange = scores[p.SeatIndex],
                RemainingCardCount = p.ServerHand.Count,
                IsWinner = (p.SeatIndex == winnerSeatIndex && !isRobFailure) || (isRobFailure && p.SeatIndex != RobberSeatIndex),
                IsRobber = (p.SeatIndex == RobberSeatIndex),
                IsRobSuccess = (p.SeatIndex == RobberSeatIndex && !isRobFailure),
                IsDoubleClose = isDoubleClose,
                IsSingleClose = isSingleClose
            };
            roundResult.PlayerResults.Add(result);

            // 更新总分
            UpdatePlayerTotalScore(p.SeatIndex, result.ScoreChange);
            
            // 更新统计数据 (胜场等)
            var existingScore = GameResult.PlayerStats.FirstOrDefault(s => s.SeatIndex == p.SeatIndex);
            if (!existingScore.Equals(default(PlayerTotalStats)))
            {
                int index = GameResult.PlayerStats.IndexOf(existingScore);
                var newStats = existingScore;
                newStats.PlayerName = p.PlayerName; // 确保名字是最新的
                if (result.IsWinner) newStats.WinCount++;
                if (isDoubleClose) newStats.DoubleCloseCount++;
                if (isSingleClose) newStats.SingleCloseCount++;
                GameResult.PlayerStats[index] = newStats;
            }
        }

        GameResult.RoundHistory.Add(roundResult);

        // 3. 发送结算 RPC (恢复调用)
        foreach (var p in seatedPlayers)
        {
            p.RpcOnRoundFinished(roundResult);
        }
        
        // 4. 同步分数给客户端
        foreach (var p in seatedPlayers)
        {
            p.RpcUpdateScores(GameResult.PlayerStats.ToArray());
        }

        // 5. 延迟开始下一局
        StartCoroutine(WaitAndStartNextRound());
    }

    [Server]
    IEnumerator WaitAndStartNextRound()
    {
        yield return new WaitForSeconds(3.0f); // 缩短等待时间
        StartNewRound();
    }

    [Server]
    void FinishGame()
    {
        Debug.Log("FinishGame called. Sending RpcOnGameFinished.");
        CurrentState = GameState.GameFinished;
        var seatedPlayers = GetSeatedPlayers();
        foreach (var p in seatedPlayers)
        {
            p.RpcOnGameFinished(GameResult);
        }
    }

    void OnTurnChanged(int oldVal, int newVal)
    {
        CurrentPlayerIndex = newVal;
        OnTurnChangedEvent?.Invoke(newVal);
        Debug.Log($"Turn Changed: {oldVal} -> {newVal}");
    }

    void OnStateChanged(GameState oldVal, GameState newVal)
    {
        CurrentState = newVal;
        OnStateChangedEvent?.Invoke(newVal);
        Debug.Log($"State Changed: {oldVal} -> {newVal}");
    }

    CardPlayer[] GetSeatedPlayers()
    {
        return FindObjectsOfType<CardPlayer>()
            .Where(p => p.SeatIndex != -1)
            .OrderBy(p => p.SeatIndex)
            .ToArray();
    }

    bool HasCards(CardPlayer player, List<Card> cardsToCheck)
    {
        List<Card> tempHand = new List<Card>(player.ServerHand);
        foreach (var c in cardsToCheck)
        {
            var found = tempHand.FirstOrDefault(h => h.Suit == c.Suit && h.Rank == c.Rank);
            if (found.Rank == 0) return false;
            tempHand.Remove(found);
        }
        return true;
    }
    
    // 【新增】添加机器人方法
    [Server]
    public void AddBot()
    {
        // 1. 找到一个空座位
        var allPlayers = FindObjectsOfType<CardPlayer>();
        List<int> takenSeats = new List<int>();
        foreach(var p in allPlayers) 
        {
            if(p.SeatIndex != -1) takenSeats.Add(p.SeatIndex);
        }

        int targetSeat = -1;
        for (int i = 0; i < 3; i++)
        {
            if (!takenSeats.Contains(i))
            {
                targetSeat = i;
                break;
            }
        }

        if (targetSeat == -1)
        {
            Debug.Log("没有空座位了");
            return;
        }

        // 2. 生成机器人对象
        if (CardPlayerPrefab == null)
        {
            Debug.LogError("CardPlayerPrefab is not assigned in PokerManager!");
            return;
        }
        
        GameObject botObj = Instantiate(CardPlayerPrefab);
        CardPlayer botPlayer = botObj.GetComponent<CardPlayer>();
        
        // 3. 标记为机器人
        botPlayer.IsBot = true;
        
        // 4. 在服务器上生成 (Spawn)
        NetworkServer.Spawn(botObj);

        // 5. 让机器人坐下
        botPlayer.SitDownInternal(targetSeat, $"电脑 {targetSeat + 1}");
        
        // 6. 检查是否可以开始游戏
        CheckAllReady();
    }
    
    // 【修改】公开 CheckAllReady 以便 AddBot 调用
    [Server]
    public void CheckAllReady()
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
            Debug.Log("所有玩家准备完毕，开始发牌...");
            InitializeGame((NetworkManager.singleton as RunFastNetworkManager)!.PendingRoomSettings);
        }
    }
}
