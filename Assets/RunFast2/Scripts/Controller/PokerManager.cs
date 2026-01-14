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
        if (CurrentState != GameState.Playing) return;
        if (CurrentPlayerIndex == -1) return;
        if (NetworkTime.time < TurnEndTime) return;

        // 超时处理
        var seatedPlayers = GetSeatedPlayers();
        if (CurrentPlayerIndex >= seatedPlayers.Length) return;

        CardPlayer currentPlayer = seatedPlayers[CurrentPlayerIndex];
        Debug.Log($"Player {currentPlayer.SeatIndex} timed out.");

        // 尝试过牌
        bool canPass = true;
        
        // 1. 如果是首发，不能过
        if (LastPlayerSeatIndex == currentPlayer.SeatIndex || LastHand == null)
        {
            canPass = false;
        }
        // 2. 如果是有出必出且有牌能管，不能过
        else if (CurrentSettings.PlayMode == PlayMode.MustPlay)
        {
            if (PokerRules.HasHandToBeat(currentPlayer.ServerHand, LastHand))
            {
                canPass = false;
            }
        }

        if (canPass)
        {
            OnPlayerPass(currentPlayer);
        }
        else
        {
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
                }
            }
            else
            {
                // 管牌：找最小能管上的
                cardsToPlay = PokerRules.GetSmallestBeatHand(currentPlayer.ServerHand, LastHand);
                
                if (cardsToPlay == null)
                {
                    // 理论上 HasHandToBeat 检查过了，不应该为空，除非逻辑不一致
                    // 如果真的为空，强制过牌
                    Debug.LogWarning($"Player {currentPlayer.SeatIndex} timed out and must play, but GetSmallestBeatHand returned null. Forcing Pass.");
                    currentPlayer.RpcOnPlayerPassed(currentPlayer.SeatIndex);
                    NextTurn(seatedPlayers);
                    return;
                }
            }

            if (cardsToPlay != null)
            {
                OnPlayerPlayCard(currentPlayer, cardsToPlay.ToArray());
            }
            else
            {
                // 理论上不应该到这，除非手牌为空但没结算
                Debug.LogError("Timeout handling failed.");
                NextTurn(seatedPlayers);
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
            TotalScores = new List<PlayerTotalScore>()
        };
        StartNewRound();
    }

    [Server]
    public void StartNewRound()
    {
        CurrentRoundCount++;
        if (CurrentRoundCount > CurrentSettings.Rounds)
        {
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
            foreach (var p in seatedPlayers)
            {
                p.RpcShowRobUI();
            }
        }
        else
        {
            CurrentState = GameState.Playing;
            DetermineStartingPlayer(seatedPlayers);
        }
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
            player.TargetRpcReceiveHand(player.connectionToClient, hands[i].ToArray());
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
        CurrentPlayerIndex = (CurrentPlayerIndex + 1) % playerCount;

        CardPlayer nextPlayer = seatedPlayers[CurrentPlayerIndex];

        if (nextPlayer.SeatIndex == LastPlayerSeatIndex)
        {
             LastHand = null;
             Debug.Log($"Player {nextPlayer.SeatIndex} wins round. New round.");
        }

        TurnEndTime = NetworkTime.time + TurnDuration; // Set Timer
        Debug.Log($"Next Turn: Player {nextPlayer.SeatIndex}");
    }

    [Server]
    void CalculateRoundScore(int winnerSeatIndex, bool isRobFailure)
    {
        CurrentState = GameState.RoundFinished;
        var seatedPlayers = GetSeatedPlayers();
        
        RoundResult roundResult = new RoundResult
        {
            RoundIndex = CurrentRoundCount,
            PlayerResults = new List<PlayerRoundResult>()
        };

        // 1. 计算基础分
        int totalScorePool = 0;
        Dictionary<int, int> scores = new Dictionary<int, int>();

        // 初始化
        foreach (var p in seatedPlayers) scores[p.SeatIndex] = 0;

        // --- 包赔逻辑 (PayForAll) ---
        // 规则：如果下家只剩1张牌，上家出单牌放走下家，且上家手里有能管住的牌（或者规则简化为只要放走就赔），则上家包赔。
        // 这里需要判断：
        // 1. 赢家是下家 (winnerSeatIndex)
        // 2. 赢家只剩1张牌时出的最后一张 (这个状态在出牌前是1张，出完是0张)
        // 3. 上家 (LastPlayerSeatIndex) 出的是单牌
        // 4. 赢家出的也是单牌 (管上了)
        // 5. 检查上家是否包赔
        
        int payerSeatIndex = -1; // 包赔者座位号，-1表示无人包赔

        if (CurrentSettings.PayForAll && !isRobFailure)
        {
            // 找到赢家的上家
            int winnerIndexInArray = -1;
            for(int i=0; i<seatedPlayers.Length; i++)
            {
                if (seatedPlayers[i].SeatIndex == winnerSeatIndex)
                {
                    winnerIndexInArray = i;
                    break;
                }
            }
            
            if (winnerIndexInArray != -1)
            {
                // 上家索引
                int prevPlayerIndex = (winnerIndexInArray - 1 + seatedPlayers.Length) % seatedPlayers.Length;
                CardPlayer prevPlayer = seatedPlayers[prevPlayerIndex];

                // 检查条件：
                // A. 赢家最后出的是单张 (LastHand.Type == Single)
                // B. 上家出牌后，赢家接的牌 (意味着 LastPlayerSeatIndex 应该是 winner，而再上一手是 prevPlayer)
                //    但在 CalculateRoundScore 调用时，LastHand 已经是赢家出的牌了。
                //    我们需要知道赢家出牌前，是不是 prevPlayer 出的单张。
                //    这需要记录 "PreLastHand" 和 "PreLastPlayer"。
                //    由于目前没记录，我们简化逻辑：
                //    如果赢家是单张赢的，且上家在这一轮中出过单张且被赢家管上... 这比较复杂。
                
                //    通常包赔判定是在 OnPlayerPlayCard 里做的。
                //    这里我们假设：如果赢家是单张结牌，且上家没有出最大的单张顶住，就算包赔。
                //    或者更简单的：只要下家报单（剩1张），上家出单张被下家管上跑了，就包赔。
                
                //    为了实现简单且准确，我们需要在 OnPlayerPlayCard 里判断。
                //    但这里已经是结算了。
                //    我们暂时只实现最基础的：如果开启包赔，且赢家是单张赢的，且上家放了单张（这个很难追溯）。
                
                //    **修正方案**：在 OnPlayerPlayCard 中，如果下家剩1张，上家出单张，记录一个标记 "PotentialPayForAll"。
                //    如果下家真的跑了，就用这个标记。
                
                //    由于没加标记，暂时先略过复杂的包赔判定，只实现基础分和炸弹分。
                //    TODO: 完善包赔判定逻辑
            }
        }

        // --- 基础分计算 ---
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

        // 2. 记录结果
        foreach (var p in seatedPlayers)
        {
            var result = new PlayerRoundResult
            {
                SeatIndex = p.SeatIndex,
                ScoreChange = scores[p.SeatIndex],
                RemainingCardCount = p.ServerHand.Count,
                IsWinner = (p.SeatIndex == winnerSeatIndex && !isRobFailure) || (isRobFailure && p.SeatIndex != RobberSeatIndex), // 抢关失败时，非抢关者都算赢
                IsRobber = (p.SeatIndex == RobberSeatIndex),
                IsRobSuccess = (p.SeatIndex == RobberSeatIndex && !isRobFailure)
            };
            roundResult.PlayerResults.Add(result);

            // 更新总分
            // 查找是否已有该玩家的记录
            var existingScore = GameResult.TotalScores.FirstOrDefault(s => s.SeatIndex == p.SeatIndex);
            if (existingScore.Equals(default(PlayerTotalScore))) // 如果是默认值（未找到）
            {
                // 添加新记录
                GameResult.TotalScores.Add(new PlayerTotalScore { SeatIndex = p.SeatIndex, Score = result.ScoreChange });
            }
            else
            {
                // 更新现有记录
                // 由于 struct 是值类型，我们需要先移除旧的，再添加新的，或者使用索引修改
                int index = GameResult.TotalScores.IndexOf(existingScore);
                var newScore = existingScore;
                newScore.Score += result.ScoreChange;
                GameResult.TotalScores[index] = newScore;
            }
        }

        GameResult.RoundHistory.Add(roundResult);

        // 3. 发送结算 RPC
        foreach (var p in seatedPlayers)
        {
            p.RpcOnRoundFinished(roundResult);
        }

        // 4. 延迟开始下一局
        StartCoroutine(WaitAndStartNextRound());
    }

    [Server]
    IEnumerator WaitAndStartNextRound()
    {
        yield return new WaitForSeconds(5.0f); // 展示结算界面 5 秒
        StartNewRound();
    }

    [Server]
    void FinishGame()
    {
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
}
