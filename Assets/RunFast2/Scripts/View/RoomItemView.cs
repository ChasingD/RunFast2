using Mirror.Discovery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RunFast2.Scripts.View
{
    public class RoomItemView : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI RoomNameText; // 显示房间名或IP
        public Button ItemButton; // 整个 Item 作为按钮
        public Image Background; // 用于改变选中颜色

        [Header("State Colors")]
        public Color NormalColor = Color.white;
        public Color SelectedColor = Color.cyan;

        public ServerResponse Info { get; private set; }
        private System.Action<RoomItemView> _onSelected;

        public void Initialize(ServerResponse info, System.Action<RoomItemView> onSelected)
        {
            Info = info;
            _onSelected = onSelected;

            // 默认显示 IP 地址
            if (RoomNameText != null)
            {
                RoomNameText.text = $"Room: {info.EndPoint.Address}"; 
            }

            if (ItemButton != null)
            {
                ItemButton.onClick.RemoveAllListeners();
                ItemButton.onClick.AddListener(OnItemClick);
            }
            
            SetSelected(false);
        }

        void OnItemClick()
        {
            _onSelected?.Invoke(this);
        }

        public void SetSelected(bool isSelected)
        {
            if (Background != null)
            {
                Background.color = isSelected ? SelectedColor : NormalColor;
            }
        }
    }
}