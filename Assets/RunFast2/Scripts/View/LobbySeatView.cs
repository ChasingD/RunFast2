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
        // public Image AvatarImage;

        [Header("Settings")]
        public int SeatID; // 0, 1, 2 (绝对座位号)

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

        public void SetState_Occupied(string playerName, bool isReady, bool isSelf)
        {
            if (SitButton) SitButton.gameObject.SetActive(false);
            if (PlayerInfoGroup) PlayerInfoGroup.SetActive(true);

            if (NameText) NameText.text = isSelf ? $"<color=yellow>{playerName}</color>" : playerName;
            if (ReadyIcon) ReadyIcon.gameObject.SetActive(isReady);
        }

        public void SetInteractable(bool canInteract)
        {
            if (SitButton) SitButton.interactable = canInteract;
        }
    }
}