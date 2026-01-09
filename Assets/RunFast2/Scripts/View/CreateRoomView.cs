using System;
using UnityEngine;
using RunFast2.Scripts.Model;

namespace RunFast2.Scripts.View
{
    // This class would manage the "Create Room" UI panel.
    // Since we don't have the visual prefab, this script serves as the logic controller
    // that would be attached to the UI root.
    public class CreateRoomView : MonoBehaviour
    {
        // Settings that would be bound to UI elements (Dropdowns, Toggles)
        // We set defaults here to match the screenshot "Run Fast"

        [Header("UI Bindings (Mock)")]
        public PlayMode SelectedPlayMode = PlayMode.MustPlay; // "有出必出"
        public int SelectedRounds = 9; // "9局"
        public FirstTurnRule SelectedFirstTurn = FirstTurnRule.Heart3; // "红桃3先出"
        public int SelectedBombScore = 5; // "5分"
        public bool ShowHandCount = true; // "显示张数"

        // Rules Checkboxes
        public bool PayForAll = true; // 放走包赔
        public bool ThreeAsBomb = false;
        public bool NoLoseOnSingle = false;
        // ... other rules

        public void OnCreateRoomClicked()
        {
            // 1. Construct the Settings object from UI state
            RoomSettings settings = new RoomSettings
            {
                DeckType = DeckType.Standard48, // Fixed for this game mode
                PlayMode = SelectedPlayMode,
                Rounds = SelectedRounds,
                FirstTurn = SelectedFirstTurn,
                BombScore = SelectedBombScore,
                ShowHandCount = ShowHandCount,

                PayForAll = PayForAll,
                ThreeAsBomb = ThreeAsBomb,
                NoLoseOnSingle = NoLoseOnSingle,
                // Map others...
            };

            Debug.Log($"Creating Room with Settings: Deck={settings.DeckType}, FirstTurn={settings.FirstTurn}");

            // 2. In a real Mirror setup, we would send a Command to the NetworkManager/Lobby
            // to create a match with these settings.
            // For this task, we call the PokerManager directly if we are the host/server.

            if (PokerManager.Instance != null)
            {
                // Note: InitializeGame is [Server] only.
                // In a client-hosted game, this works. In dedicated, we need a Command.
                // Assuming Client-Hosted for development testing:
                PokerManager.Instance.InitializeGame(settings);
            }
            else
            {
                Debug.LogWarning("PokerManager instance not found. Are you in the GameScene?");
            }
        }
    }
}
