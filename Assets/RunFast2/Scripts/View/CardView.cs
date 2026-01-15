using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using RunFast2.Scripts.Manager; // 引用 Manager
using DG.Tweening; // 引入 DOTween

namespace RunFast2.Scripts.View
{
    public class CardView : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI References")]
        public Image CardImage;       // Background/Sprite
        public RectTransform VisualRoot; // 新增：用于位移的子物体

        [Header("State")]
        public Card CardData;
        public bool IsSelected = false;

        private RectTransform _rectTransform;
        private float _originalY;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            
            // 如果没有手动赋值 VisualRoot，尝试查找名为 "Visual" 的子物体
            if (VisualRoot == null)
            {
                var visual = transform.Find("Visual");
                if (visual != null) VisualRoot = visual as RectTransform;
            }
        }

        private void Start()
        {
            // 记录初始 Y 位置
            // 如果有 VisualRoot，记录 VisualRoot 的本地 Y
            // 否则记录自身的 anchoredPosition Y (虽然这在 LayoutGroup 下不稳定)
            if (VisualRoot != null)
            {
                _originalY = VisualRoot.localPosition.y;
            }
            else if (_rectTransform != null)
            {
                _originalY = _rectTransform.anchoredPosition.y;
            }
        }

        public void Initialize(Card card)
        {
            this.CardData = card;
            UpdateVisuals();
        }

        void UpdateVisuals()
        {
            // 尝试从 Manager 获取 Sprite
            if (CardAssetManager.Instance != null && CardImage != null)
            {
                Sprite sprite = CardAssetManager.Instance.GetCardSprite(CardData.Suit, CardData.Rank);
                if (sprite != null)
                {
                    CardImage.sprite = sprite;
                }
                else
                {
                    Debug.LogWarning($"Sprite not found for {CardData.Suit} {CardData.Rank}");
                }
            }

            // Name for debugging
            gameObject.name = $"Card_{CardData.Suit}_{CardData.Rank}";
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            ToggleSelection();
        }

        public void ToggleSelection()
        {
            IsSelected = !IsSelected;

            float offset = 30f; // 弹起高度
            float duration = 0.2f; // 动画时间

            if (VisualRoot != null)
            {
                // 方案 B：移动子物体 (推荐)
                // 使用 localPosition，不受 LayoutGroup 影响
                float targetY = IsSelected ? _originalY + offset : _originalY;
                
                // 使用 DOTween 移动
                VisualRoot.DOLocalMoveY(targetY, duration).SetEase(Ease.OutQuad);
            }
            else if (_rectTransform != null)
            {
                // 方案 A (回退)：修改 Pivot (如果不想改 Prefab 结构)
                // 假设默认 Pivot 是 (0.5, 0.5)
                // 这种方式比较 hacky，且依赖于 LayoutGroup 的具体设置
                // _rectTransform.pivot = IsSelected ? new Vector2(0.5f, 0.4f) : new Vector2(0.5f, 0.5f);
                
                // 或者继续尝试修改 anchoredPosition，但在 LayoutGroup 下通常会失效或抖动
                // 建议务必使用 VisualRoot 方案
                Debug.LogWarning("CardView: Missing VisualRoot. Selection animation might be buggy with LayoutGroup.");
            }
        }
    }
}