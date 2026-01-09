using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace RunFast2.Scripts.View
{
    public class CardView : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI References")]
        public Image CardImage;       // Background/Sprite
        public TextMeshProUGUI RankText;
        public TextMeshProUGUI SuitText;

        [Header("State")]
        public Card CardData;
        public bool IsSelected = false;

        private RectTransform _rectTransform;
        private float _originalY;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
            if (_rectTransform != null)
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
            // Simple Text Fallback if no sprites
            if (RankText != null) RankText.text = GetRankString(CardData.Rank);
            if (SuitText != null)
            {
                SuitText.text = GetSuitSymbol(CardData.Suit);
                // Set color
                Color c = (CardData.Suit == CardSuit.Heart || CardData.Suit == CardSuit.Diamond) ? Color.red : Color.black;
                SuitText.color = c;
                if (RankText != null) RankText.color = c;
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

            // Visual Pop Effect
            if (_rectTransform != null)
            {
                float targetY = IsSelected ? _originalY + 20f : _originalY;
                _rectTransform.anchoredPosition = new Vector2(_rectTransform.anchoredPosition.x, targetY);
            }
        }

        // Helpers
        string GetRankString(CardRank rank)
        {
            if (rank <= CardRank.Ten) return ((int)rank).ToString();
            return rank.ToString()[0].ToString(); // J, Q, K, A, 2
        }

        string GetSuitSymbol(CardSuit suit)
        {
            switch (suit)
            {
                case CardSuit.Diamond: return "♦";
                case CardSuit.Club: return "♣";
                case CardSuit.Heart: return "♥";
                case CardSuit.Spade: return "♠";
                default: return "?";
            }
        }
    }
}
