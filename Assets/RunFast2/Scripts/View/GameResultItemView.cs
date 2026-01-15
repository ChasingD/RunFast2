using RunFast2.Scripts.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RunFast2.Scripts.View
{
    public class GameResultItemView : MonoBehaviour
    {
        public TextMeshProUGUI NameText;
        public TextMeshProUGUI TotalScoreText;
        public TextMeshProUGUI WinCountText;
        public TextMeshProUGUI BombCountText;
        public TextMeshProUGUI CloseCountText; // 显示双关/单关次数
        public Image Background; // 赢家高亮

        public void SetData(PlayerTotalStats data)
        {
            if (NameText) NameText.text = data.PlayerName;
            
            if (TotalScoreText)
            {
                TotalScoreText.text = data.TotalScore > 0 ? $"+{data.TotalScore}" : data.TotalScore.ToString();
                TotalScoreText.color = data.TotalScore > 0 ? Color.yellow : Color.white;
            }

            if (WinCountText) WinCountText.text = $"胜局: {data.WinCount}";
            if (BombCountText) BombCountText.text = $"炸弹: {data.BombCount}";
            if (CloseCountText) CloseCountText.text = $"双关: {data.DoubleCloseCount} / 单关: {data.SingleCloseCount}";

            if (Background)
            {
                // 总分最高者高亮
                // 这里简单判断是否为正分，实际应该比较所有玩家
                Background.color = data.TotalScore > 0 ? new Color(1, 0.8f, 0, 0.5f) : new Color(0, 0, 0, 0.5f);
            }
        }
    }
}