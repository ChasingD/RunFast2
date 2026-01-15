using System;
using System.Collections.Generic;

namespace RunFast2.Scripts.Model
{
    [Serializable]
    public struct PlayerRoundResult
    {
        public int SeatIndex;
        public string PlayerName; // 增加名字
        public int ScoreChange; // 本局得分变化
        public int RemainingCardCount; // 剩余牌数
        public bool IsWinner;
        public bool IsRobber; // 是否是抢关者
        public bool IsRobSuccess; // 抢关是否成功
        public bool IsDoubleClose; // 是否双关 (剩余牌数未动且>=10张，或者特定规则)
        public bool IsSingleClose; // 是否单关 (剩余牌数未动但<10张，或者特定规则)
    }

    [Serializable]
    public struct RoundResult
    {
        public int RoundIndex;
        public List<PlayerRoundResult> PlayerResults;
    }

    [Serializable]
    public struct PlayerTotalStats
    {
        public int SeatIndex;
        public string PlayerName;
        public int TotalScore;
        public int WinCount;
        public int DoubleCloseCount; // 双关次数
        public int SingleCloseCount; // 单关次数
        public int BombCount; // 炸弹次数
    }

    [Serializable]
    public struct GameTotalResult
    {
        public List<RoundResult> RoundHistory;
        public List<PlayerTotalStats> PlayerStats; 
    }
}