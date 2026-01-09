using System;
using System.Collections.Generic;

namespace RunFast2.Scripts.Model
{
    [Serializable]
    public enum DeckType
    {
        Standard48 = 0, // 48 cards: Remove 3x2 (Diamond, Club, Heart), Remove Spade A. Keep Spade 2.
        // Future types can be added here
    }

    [Serializable]
    public enum FirstTurnRule
    {
        Heart3 = 0,     // Player with Heart 3 goes first
        Rotate = 1,     // Rotate clockwise
        Winner = 2      // Winner of previous round goes first
    }

    [Serializable]
    public enum PlayMode
    {
        MustPlay = 0,   // 有出必出
        OptionalPlay = 1 // 非必出
    }

    [Serializable]
    public class RoomSettings
    {
        public DeckType DeckType = DeckType.Standard48;
        public PlayMode PlayMode = PlayMode.MustPlay;
        public int Rounds = 9; // 9, 18, 36
        public FirstTurnRule FirstTurn = FirstTurnRule.Heart3;
        public int BombScore = 5; // 0, 5, 10
        public bool ShowHandCount = true;

        // Extra Rules (Checkboxes)
        public bool PayForAll = true; // 放走包赔
        public bool ThreeAsBomb = false; // 三A算炸
        public bool NoLoseOnSingle = false; // 报单不输
        public bool ReversePass = false; // 反关
        public bool RobPass = false; // 抢关
        public bool TailSwing = false; // 甩尾
        public bool OpenDoorSee3 = false; // 开门见3
        public bool VoiceChat = false;
        public bool CardRecorder = false;
    }
}
