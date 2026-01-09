using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RunFast2.Scripts.View
{
    public class SeatView : MonoBehaviour
    {
        [Header("UI Components")]
        public Button SitButton;           // “入座”按钮 (没人时显示)
        public GameObject PlayerInfoGroup; // 玩家信息面板 (有人时显示)
        public TextMeshProUGUI NameText;   // 玩家名字
        public Image ReadyIcon;            // 准备状态图标
        // public Image AvatarImage;          // 头像

        [Header("Settings")]
        public int SeatID;                 // 0, 1, 2 (在Inspector里手动填好)

        // 定义一个委托，当点击入座时通知上层
        public System.Action<int> OnSitClicked;

        private void Start()
        {
            SitButton.onClick.AddListener(() => OnSitClicked?.Invoke(SeatID));
            // 初始状态：空座位
            SetState_Empty();
        }

        /// <summary>
        /// 状态：空座位
        /// </summary>
        public void SetState_Empty()
        {
            SitButton.gameObject.SetActive(true);
            SitButton.interactable = true; // 默认可点
            if (PlayerInfoGroup != null) PlayerInfoGroup.SetActive(false);
            ReadyIcon.gameObject.SetActive(false);
        }

        /// <summary>
        /// 状态：有人入座
        /// </summary>
        public void SetState_Occupied(string playerName, bool isReady, bool isSelf)
        {
            SitButton.gameObject.SetActive(false); // 隐藏入座按钮
            if (PlayerInfoGroup != null) PlayerInfoGroup.SetActive(true); // 显示玩家信息

            NameText.text = isSelf ? $"<color=yellow>{playerName} (Me)</color>" : playerName;
            ReadyIcon.gameObject.SetActive(isReady);
        }

        /// <summary>
        /// 锁定按钮（例如玩家已经坐在别的地方了，其他空位就不能点了）
        /// </summary>
        public void SetInteractable(bool canInteract)
        {
            SitButton.interactable = canInteract;
        }
    }
}