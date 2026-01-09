using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using RunFast2.Scripts.Network;
using System.Linq;

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

        [Header("Runtime State")]
        public List<CardView> CurrentCardViews = new List<CardView>();
        private CardPlayer _localPlayer;

        private void Start()
        {
            // Initial State
            if (ActionPanel) ActionPanel.SetActive(false);
            if (PlayButton) PlayButton.onClick.AddListener(OnPlayClicked);
            if (PassButton) PassButton.onClick.AddListener(OnPassClicked);

            // Subscribe to Events
            CardPlayer.OnHandReceived += RefreshHand;
            CardPlayer.OnPlayerInfoUpdated += OnPlayerUpdated; // To catch when local player is ready/set
            PokerManager.OnTurnChangedEvent += OnTurnChanged;

            // Try to find local player if already exists
            FindLocalPlayer();
        }

        private void OnDestroy()
        {
            CardPlayer.OnHandReceived -= RefreshHand;
            CardPlayer.OnPlayerInfoUpdated -= OnPlayerUpdated;
            PokerManager.OnTurnChangedEvent -= OnTurnChanged;
        }

        private void Update()
        {
            // Polling fallback to find local player if not set
            if (_localPlayer == null) FindLocalPlayer();
        }

        void FindLocalPlayer()
        {
            var players = FindObjectsOfType<CardPlayer>();
            foreach(var p in players)
            {
                if (p.isLocalPlayer)
                {
                    _localPlayer = p;
                    RefreshHand(); // Initial refresh
                    CheckTurnButtons();
                    break;
                }
            }
        }

        // --- Event Handlers ---

        void OnPlayerUpdated(CardPlayer player)
        {
            if (player.isLocalPlayer)
            {
                _localPlayer = player;
                CheckTurnButtons();
            }
        }

        void OnTurnChanged(int seatIndex)
        {
            CheckTurnButtons();
        }

        void RefreshHand()
        {
            if (_localPlayer == null) return;
            // if (_localPlayer.MyHand.Count == 0) return; // Maybe keep empty container?

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

            // Create new views
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

        void CheckTurnButtons()
        {
            if (_localPlayer == null || PokerManager.Instance == null)
            {
                if (ActionPanel) ActionPanel.SetActive(false);
                return;
            }

            // Check if it's my turn
            int currentTurnIndex = PokerManager.Instance.CurrentPlayerIndex;
            bool isMyTurn = (currentTurnIndex == _localPlayer.SeatIndex);

            if (ActionPanel) ActionPanel.SetActive(isMyTurn);

            // Optional: Disable Pass button if I am the Round Leader
            if (isMyTurn && PassButton != null)
            {
                bool isRoundLeader = (PokerManager.Instance.LastPlayerSeatIndex == _localPlayer.SeatIndex)
                                     || (PokerManager.Instance.LastHand == null);
                PassButton.interactable = !isRoundLeader;
            }
        }

        // --- Button Actions ---

        void OnPlayClicked()
        {
            if (_localPlayer == null) return;

            // Gather selected cards
            List<Card> selectedCards = new List<Card>();
            foreach (var view in CurrentCardViews)
            {
                if (view.IsSelected)
                {
                    selectedCards.Add(view.CardData);
                }
            }

            if (selectedCards.Count == 0)
            {
                Debug.Log("Please select cards to play.");
                return;
            }

            // Send Command
            _localPlayer.CmdPlayCard(selectedCards.ToArray());
        }

        void OnPassClicked()
        {
            if (_localPlayer == null) return;
            _localPlayer.CmdPass();
        }
    }
}
