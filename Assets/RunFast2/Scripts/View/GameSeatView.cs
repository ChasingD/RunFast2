using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace RunFast2.Scripts.View
{
    public class GameSeatView : MonoBehaviour
    {
        [Header("UI Components")]
        public GameObject PlayerInfoGroup; 
        public TextMeshProUGUI NameText;   
        public TextMeshProUGUI ScoreText;  
        // public Image AvatarImage;
        public Button AvatarButton; // 用于点击头像使用道具

        [Header("Active Indicator")]
        public GameObject ActiveIndicator; 
        public GameObject Clock;
        public TextMeshProUGUI TimerText;  

        [Header("Played Cards Area")]
        public Transform PlayedCardsContainer; 
        public GameObject CardViewPrefab;      
        public GameObject ActionBubble;        
        public TextMeshProUGUI ActionText;     
        
        [Header("Effects")]
        public TextMeshProUGUI ScoreChangeText; 

        [Header("Settings")]
        public int UIIndex; // 0=Bottom, 1=Right, 2=Left

        public System.Action<int> OnAvatarClicked; // 回传 UIIndex

        private void Start()
        {
            if (AvatarButton) AvatarButton.onClick.AddListener(() => OnAvatarClicked?.Invoke(UIIndex));
            
            // 初始隐藏
            gameObject.SetActive(false);
        }

        public void SetState_Occupied(string playerName, bool isSelf, int cardCount, int score)
        {
            gameObject.SetActive(true);
            if (PlayerInfoGroup) PlayerInfoGroup.SetActive(true);

            string countStr = cardCount > 0 ? $" [{cardCount}]" : "";
            if (NameText) NameText.text = (isSelf ? $"<color=yellow>{playerName}</color>" : playerName) + countStr;
            
            if (ScoreText) ScoreText.text = score.ToString();
        }

        public void SetActiveState(bool isActive)
        {
            if (ActiveIndicator) ActiveIndicator.SetActive(isActive);
            if (Clock) Clock.SetActive(isActive);
            if (TimerText) TimerText.gameObject.SetActive(isActive);
        }

        public void UpdateTimer(float remainingTime)
        {
            if (TimerText != null && TimerText.gameObject.activeSelf)
            {
                TimerText.text = Mathf.CeilToInt(remainingTime).ToString();
                TimerText.color = remainingTime <= 5 ? Color.red : Color.white;
            }
        }

        public void UpdateScore(int newScore, int change)
        {
            if (ScoreText) ScoreText.text = newScore.ToString();
            
            if (change != 0 && ScoreChangeText)
            {
                ShowScoreChangeEffect(change).Forget();
            }
        }

        async UniTaskVoid ShowScoreChangeEffect(int change)
        {
            if (ScoreChangeText == null) return;
            ScoreChangeText.gameObject.SetActive(true);
            ScoreChangeText.text = change > 0 ? $"+{change}" : change.ToString();
            ScoreChangeText.color = change > 0 ? Color.yellow : Color.red;
            
            var rect = ScoreChangeText.rectTransform;
            Vector2 startPos = new Vector2(0, 50); 
            rect.anchoredPosition = startPos;
            ScoreChangeText.alpha = 1f;

            var sequence = DOTween.Sequence();
            sequence.Append(rect.DOAnchorPosY(startPos.y + 50, 1.5f).SetEase(Ease.OutQuad));
            sequence.Join(ScoreChangeText.DOFade(0, 1.5f).SetEase(Ease.InQuad));
            
            await sequence.AsyncWaitForCompletion();
            
            if (ScoreChangeText != null) ScoreChangeText.gameObject.SetActive(false);
        }

        public void ShowPlayedCards(Card[] cards)
        {
            ClearPlayedCards();
            if (ActionBubble) ActionBubble.SetActive(false);

            if (cards == null || cards.Length == 0) return;

            if (PlayedCardsContainer == null || CardViewPrefab == null) return;

            // 使用 UniTask 启动异步方法
            AnimateShowCardsAsync(cards).Forget();
        }

        async UniTaskVoid AnimateShowCardsAsync(Card[] cards)
        {
            Transform animRoot = PlayedCardsContainer.parent; 
            Vector3 startWorldPos = PlayerInfoGroup != null ? PlayerInfoGroup.transform.position : transform.position;

            List<GameObject> placeholders = new List<GameObject>();
            List<Vector3> targetPositions = new List<Vector3>();

            foreach (var card in cards)
            {
                GameObject placeholder = Instantiate(CardViewPrefab, PlayedCardsContainer);
                var canvasGroup = placeholder.GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = placeholder.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 0; 
                placeholders.Add(placeholder);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(PlayedCardsContainer as RectTransform);

            foreach (var p in placeholders)
            {
                targetPositions.Add(p.transform.position);
            }

            List<GameObject> realCards = new List<GameObject>();
            List<UniTask> animTasks = new List<UniTask>();

            for (int i = 0; i < cards.Length; i++)
            {
                var card = cards[i];
                Vector3 targetPos = targetPositions[i];

                GameObject go = Instantiate(CardViewPrefab, animRoot);
                go.transform.position = startWorldPos;
                go.transform.localScale = Vector3.zero; 

                CardView view = go.GetComponent<CardView>();
                if (view != null)
                {
                    view.Initialize(card);
                    var btn = view.GetComponent<Button>();
                    if (btn) Destroy(btn);
                    if (view.CardImage) view.CardImage.raycastTarget = false;
                }
                
                realCards.Add(go);

                var seq = DOTween.Sequence();
                seq.Append(go.transform.DOMove(targetPos, 0.3f).SetEase(Ease.OutBack)); 
                seq.Join(go.transform.DOScale(Vector3.one * 0.7f, 0.3f)); 
                seq.SetDelay(i * 0.05f);
                
                animTasks.Add(seq.AsyncWaitForCompletion().AsUniTask());
            }

            await UniTask.WhenAll(animTasks);

            for (int i = 0; i < realCards.Count; i++)
            {
                if (realCards[i] != null && placeholders[i] != null)
                {
                    realCards[i].transform.SetParent(PlayedCardsContainer);
                    realCards[i].transform.SetSiblingIndex(placeholders[i].transform.GetSiblingIndex());
                    realCards[i].transform.localScale = Vector3.one * 0.7f;
                }
            }

            foreach (var p in placeholders)
            {
                if (p != null) Destroy(p);
            }
        }

        public void ShowActionText(string text)
        {
            ClearPlayedCards();
            if (ActionBubble) ActionBubble.SetActive(true);
            if (ActionText) ActionText.text = text;
        }

        public void ClearPlayedCards()
        {
            this.transform.DOKill(true);
            
            if (PlayedCardsContainer != null)
            {
                foreach (Transform child in PlayedCardsContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            if (ActionBubble) ActionBubble.SetActive(false);
        }
    }
}