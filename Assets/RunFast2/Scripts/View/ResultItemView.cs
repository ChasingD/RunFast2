using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RunFast2.Scripts.View
{
    public class ResultItemView : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI PlayerNameText;
        public TextMeshProUGUI ScoreText;
        public TextMeshProUGUI StatsText; // 用于显示双关、单关等信息

        // 用于单局结算
        public void Initialize(RunFast2.Scripts.Model.PlayerRoundResult result)
        {
            if (PlayerNameText) PlayerNameText.text = result.PlayerName;
            if (ScoreText) ScoreText.text = result.ScoreChange.ToString("+#;-#;0");
            
            string stats = $"剩余: {result.RemainingCardCount}";
            if (result.IsDoubleClose) stats += " (双关)";
            if (result.IsSingleClose) stats += " (单关)";
            if (StatsText) StatsText.text = stats;
        }

        // 用于总结算
        public void Initialize(RunFast2.Scripts.Model.PlayerTotalStats stats)
        {
            if (PlayerNameText) PlayerNameText.text = stats.PlayerName;
            if (ScoreText) ScoreText.text = stats.TotalScore.ToString();
            
            string statsInfo = $"胜利: {stats.WinCount} | 炸弹: {stats.BombCount}\n双关: {stats.DoubleCloseCount} | 单关: {stats.SingleCloseCount}";
            if (StatsText) StatsText.text = statsInfo;
        }
    }
}