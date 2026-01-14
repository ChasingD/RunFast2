using System;
using System.Collections.Generic;

namespace RunFast2.Scripts.Model
{
    [Serializable]
    public struct PlayerRoundResult
    {
        public int SeatIndex;
        public int ScoreChange; // 本局得分变化
        public int RemainingCardCount; // 剩余牌数
        public bool IsWinner;
        public bool IsRobber; // 是否是抢关者
        public bool IsRobSuccess; // 抢关是否成功
    }

    [Serializable]
    public struct RoundResult
    {
        public int RoundIndex;
        public List<PlayerRoundResult> PlayerResults;
    }

    [Serializable]
    public struct PlayerTotalScore
    {
        public int SeatIndex;
        public int Score;
    }

    [Serializable]
    public struct GameTotalResult
    {
        public List<RoundResult> RoundHistory;
        public List<PlayerTotalScore> TotalScores; 
    }
}