using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using RunFast2.Scripts.Network;
using RunFast2.Scripts.Model;

public class PokerManager : NetworkBehaviour
{
    public static PokerManager Instance;

    private List<Card> _deck = new List<Card>();

    [SyncVar]
    public int CurrentPlayerIndex = -1; // Index in the seatedPlayers array

    public RoomSettings CurrentSettings = new RoomSettings(); // Default settings

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

        InitDeck();
        ShuffleDeck();
    
        // Deal cards and get the hands back so we can check who has what
        var hands = DealCards(seatedPlayers);

        // Determine First Player
        DetermineStartingPlayer(seatedPlayers, hands);
    }

    [Server]
    void InitDeck()
    {
        _deck.Clear();

        if (CurrentSettings.DeckType == DeckType.Standard48)
        {
            // Logic: 48 Cards
            // Remove: 3x2 (Diamond, Club, Heart), 1x Spade Ace
            // Keep: Spade 2

            foreach (CardSuit suit in System.Enum.GetValues(typeof(CardSuit)))
            {
                foreach (CardRank rank in System.Enum.GetValues(typeof(CardRank)))
                {
                    // CardRank: Three=3 ... Ace=14, Two=15

                    if (rank == CardRank.Two)
                    {
                        // Remove if NOT Spade (Remove Diamond/Club/Heart 2)
                        if (suit != CardSuit.Spade) continue;
                    }
                    else if (rank == CardRank.Ace)
                    {
                        // Remove if Spade (Remove Spade Ace)
                        if (suit == CardSuit.Spade) continue;
                    }

                    _deck.Add(new Card(suit, rank));
                }
            }
        }
        else
        {
            // Default 52 cards fallback
            foreach (CardSuit suit in System.Enum.GetValues(typeof(CardSuit)))
            {
                foreach (CardRank rank in System.Enum.GetValues(typeof(CardRank)))
                {
                    _deck.Add(new Card(suit, rank));
                }
            }
        }

        Debug.Log($"Deck Initialized with {_deck.Count} cards.");
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
    List<List<Card>> DealCards(CardPlayer[] players)
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
            player.TargetRpcReceiveHand(player.connectionToClient, hands[i].ToArray());
            Debug.Log($"Server sent {hands[i].Count} cards to Player {player.netId} (Seat {player.SeatIndex})");
        }

        return hands;
    }

    [Server]
    void DetermineStartingPlayer(CardPlayer[] players, List<List<Card>> hands)
    {
        int startingIndex = 0; // Default to first player

        if (CurrentSettings.FirstTurn == FirstTurnRule.Heart3)
        {
            // Find who has Heart 3
            bool found = false;
            for (int i = 0; i < hands.Count; i++)
            {
                foreach (var card in hands[i])
                {
                    if (card.Suit == CardSuit.Heart && card.Rank == CardRank.Three)
                    {
                        startingIndex = i;
                        found = true;
                        Debug.Log($"Player {players[i].SeatIndex} has Heart 3 and starts.");
                        break;
                    }
                }
                if (found) break;
            }
            if (!found)
            {
                Debug.LogWarning("Heart 3 not found in any hand! (Maybe 2 player game?) Defaulting to index 0.");
            }
        }
        else if (CurrentSettings.FirstTurn == FirstTurnRule.Rotate)
        {
            // Needs game state to know who was previous, for now random or 0
             Debug.Log("FirstTurnRule: Rotate. (Not fully implemented, using 0)");
        }
        else if (CurrentSettings.FirstTurn == FirstTurnRule.Winner)
        {
             Debug.Log("FirstTurnRule: Winner. (Not fully implemented, using 0)");
        }

        CurrentPlayerIndex = startingIndex;
        // Notify players whose turn it is?
        // players[CurrentPlayerIndex].TargetRpcYourTurn(...)
        // For now just setting the SyncVar is enough for clients to know if they observe it.
    }
}
