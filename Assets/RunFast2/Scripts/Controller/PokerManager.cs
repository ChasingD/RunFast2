using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using RunFast2.Scripts.Network;
using RunFast2.Scripts.Model;
using RunFast2.Scripts.Logic;

public class PokerManager : NetworkBehaviour
{
    public static PokerManager Instance;

    private List<Card> _deck = new List<Card>();

    [SyncVar(hook = nameof(OnTurnChanged))]
    public int CurrentPlayerIndex = -1; // Index in the seatedPlayers array

    // Current Round State
    public PokerHand LastHand = null;
    public int LastPlayerSeatIndex = -1; // The seat index of the player who played LastHand

    public RoomSettings CurrentSettings = new RoomSettings();

    // Events for UI (Local)
    public static event System.Action<int> OnTurnChangedEvent;

    private void Awake()
    {
        Instance = this;
    }

    [Server]
    public void InitializeGame(RoomSettings settings)
    {
        CurrentSettings = settings;
        StartGame();
    }

    [Server]
    public void StartGame()
    {
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

        LastHand = null;
        LastPlayerSeatIndex = -1;

        InitDeck();
        ShuffleDeck();
    
        DealCards(seatedPlayers);
        DetermineStartingPlayer(seatedPlayers);
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
            player.TargetRpcReceiveHand(player.connectionToClient, hands[i].ToArray());
        }
    }

    [Server]
    void DetermineStartingPlayer(CardPlayer[] players)
    {
        int startingIndex = 0;

        if (CurrentSettings.FirstTurn == FirstTurnRule.Heart3)
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

        CurrentPlayerIndex = startingIndex;
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

        LastHand = hand;
        LastPlayerSeatIndex = player.SeatIndex;

        // Correct RPC Call: Invoke ClientRpc on the NetworkBehaviour
        player.RpcOnPlayerPlayed(player.SeatIndex, cards, (int)hand.Type);

        if (player.ServerHand.Count == 0)
        {
            player.RpcGameFinished(player.SeatIndex);
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

        if (player != currentPlayer) return;

        if (LastPlayerSeatIndex == player.SeatIndex)
        {
            Debug.LogWarning($"Player {player.SeatIndex} is round leader and cannot pass.");
            return;
        }

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

        Debug.Log($"Next Turn: Player {nextPlayer.SeatIndex}");
    }

    void OnTurnChanged(int oldVal, int newVal)
    {
        CurrentPlayerIndex = newVal;
        OnTurnChangedEvent?.Invoke(newVal);
        Debug.Log($"Turn Changed: {oldVal} -> {newVal}");
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
