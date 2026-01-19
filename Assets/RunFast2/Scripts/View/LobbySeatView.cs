using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RunFast2.Scripts.View
{
    public class LobbySeatView : MonoBehaviour
    {
        [Header("UI Components")]
        public Button SitButton;           // “入座”按钮
        public GameObject PlayerInfoGroup; // 玩家信息面板
        public TextMeshProUGUI NameText;   // 玩家名字
        public Image ReadyIcon;            // 准备状态图标
        public Image AvatarImage;          // 头像图片

        [Header("Settings")]
        public int SeatID; // 0, 1, 2 (绝对座位号)
        public Sprite BotAvatarSprite; // 机器人头像 (可选，在 Inspector 中赋值)
        public Sprite DefaultAvatarSprite; // 默认头像

        public System.Action<int> OnSitClicked;

        private void Start()
        {
            if (SitButton) SitButton.onClick.AddListener(() => OnSitClicked?.Invoke(SeatID));
            SetState_Empty();
        }

        public void SetState_Empty()
        {
            if (SitButton)
            {
                SitButton.gameObject.SetActive(true);
                SitButton.interactable = true;
            }
            if (PlayerInfoGroup) PlayerInfoGroup.SetActive(false);
            if (ReadyIcon) ReadyIcon.gameObject.SetActive(false);
            if (NameText) NameText.text = "";
        }

        public void SetState_Occupied(string playerName, bool isReady, bool isSelf, bool isBot = false)
        {
            if (SitButton) SitButton.gameObject.SetActive(false);
            if (PlayerInfoGroup) PlayerInfoGroup.SetActive(true);

            // 名字颜色处理
            string displayName = playerName;
            if (isBot) displayName = $"<color=#00FFFF>[Bot]</color> {playerName}"; // 使用十六进制颜色
            else if (isSelf) displayName = $"<color=#FFFF00>{playerName}</color>"; // 使用十六进制颜色
            
            if (NameText) NameText.text = displayName;
            if (ReadyIcon) ReadyIcon.gameObject.SetActive(isReady);
            
            // 头像处理
            if (AvatarImage)
            {
                if (isBot && BotAvatarSprite != null)
                    AvatarImage.sprite = BotAvatarSprite;
                else if (DefaultAvatarSprite != null)
                    AvatarImage.sprite = DefaultAvatarSprite;
            }
        }

        public void SetInteractable(bool canInteract)
        {
            if (SitButton) SitButton.interactable = canInteract;
        }
    }
}