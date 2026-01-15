using RunFast2.Scripts.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RunFast2.Scripts.View
{
    public class RoundResultItemView : MonoBehaviour
    {
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI ScoreText;
        public TextMeshProUGUI InfoText; // 显示剩余张数、关门等信息
        public Image Background; // 赢家高亮
        public Color winColor, loseColor;

        public void SetData(PlayerRoundResult data)
        {
            if (NameText) NameText.text = data.PlayerName;
            
            if (ScoreText)
            {
                ScoreText.text = data.ScoreChange > 0 ? $"+{data.ScoreChange}" : data.ScoreChange.ToString();
                ScoreText.color = data.ScoreChange > 0 ? Color.yellow : Color.white;
            }

            if (InfoText)
            {
                string info = $"剩{data.RemainingCardCount}张";
                if (data.IsDoubleClose) info += " [双关]";
                else if (data.IsSingleClose) info += " [单关]";
                if (data.IsRobber) info += data.IsRobSuccess ? " [抢关成功]" : " [抢关失败]";
                
                InfoText.text = info;
            }

            if (Background)
            {
                // 简单的赢家高亮逻辑
                Background.color = data.IsWinner ? winColor: loseColor;
            }
        }
    }
}