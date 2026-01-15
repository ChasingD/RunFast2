using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
// 引用 UniTask

// 引入 DOTween

namespace RunFast2.Scripts.View
{
    public class SeatView : MonoBehaviour
    {
        [Header("UI Components")]
        public Button SitButton;           // “入座”按钮 (没人时显示)
        public GameObject PlayerInfoGroup; // 玩家信息面板 (有人时显示)
        public TextMeshProUGUI NameText;   // 玩家名字
        public TextMeshProUGUI ScoreText;  // 玩家总分 (新增)
        public Image ReadyIcon;            // 准备状态图标
        // public Image AvatarImage;          // 头像

        [Header("Active Indicator")]
        public GameObject ActiveIndicator; // 当前出牌指示器 (如高亮框)
        public GameObject Clock;
        public TextMeshProUGUI TimerText;  // 倒计时文本

        [Header("Played Cards Area")]
        public Transform PlayedCardsContainer; // 出牌区域容器
        public GameObject CardViewPrefab;      // 牌的预制体
        public GameObject ActionBubble;        // 说话框父物体
        public TextMeshProUGUI ActionText;     // 显示"不要"、"抢关"等文字
        
        [Header("Effects")]
        public TextMeshProUGUI ScoreChangeText; // 用于飘字显示分数变化 (新增)

        [Header("Settings")]
        public int SeatID;                 // 0, 1, 2 (在Inspector里手动填好)

        // 定义一个委托，当点击入座时通知上层
        public Action<int> OnSitClicked;

        private void Start()
        {
            if (SitButton) SitButton.onClick.AddListener(() => OnSitClicked?.Invoke(SeatID));
            // 初始状态：空座位
            SetState_Empty();
            ClearPlayedCards();
            if (ScoreChangeText) ScoreChangeText.gameObject.SetActive(false);
            if (ActiveIndicator) ActiveIndicator.SetActive(false);
            if (Clock) Clock.SetActive(false);
            if (TimerText) TimerText.gameObject.SetActive(false);
        }

        /// <summary>
        /// 状态：空座位
        /// </summary>
        public void SetState_Empty()
        {
            if (SitButton)
            {
                SitButton.gameObject.SetActive(true);
                SitButton.interactable = true; // 默认可点
            }
            if (PlayerInfoGroup != null) PlayerInfoGroup.SetActive(false);
            if (ReadyIcon) ReadyIcon.gameObject.SetActive(false);
            if (NameText) NameText.text = ""; // 彻底清空名字
            if (ScoreText) ScoreText.text = "0";
            if (ActiveIndicator) ActiveIndicator.SetActive(false);
            if (Clock) Clock.SetActive(false);
            if (TimerText) TimerText.gameObject.SetActive(false);
            ClearPlayedCards();
        }

        /// <summary>
        /// 状态：有人入座
        /// </summary>
        public void SetState_Occupied(string playerName, bool isReady, bool isSelf, int cardCount, int score = 0)
        {
            if (SitButton) SitButton.gameObject.SetActive(false); // 隐藏入座按钮
            if (PlayerInfoGroup != null) PlayerInfoGroup.SetActive(true); // 显示玩家信息

            // 显示名字和剩余牌数
            string countStr = cardCount > 0 ? $" [{cardCount}]" : "";
            if (NameText) NameText.text = (isSelf ? $"<color=yellow>{playerName}</color>" : playerName) + countStr;
            
            if (ScoreText) ScoreText.text = score.ToString();
            
            // 游戏开始后隐藏 ReadyIcon，这里由外部控制，或者根据 isReady 状态
            // 如果游戏正在进行中，isReady 应该为 false (或者我们忽略它)
            // 简单起见，这里只显示 isReady，外部逻辑负责在游戏开始时将 isReady 设为 false
            if (ReadyIcon) ReadyIcon.gameObject.SetActive(isReady);
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

        /// <summary>
        /// 更新分数并显示飘字
        /// </summary>
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
            ScoreChangeText.gameObject.SetActive(true);
            ScoreChangeText.text = change > 0 ? $"+{change}" : change.ToString();
            ScoreChangeText.color = change > 0 ? Color.yellow : Color.red;
            
            // 使用 DOTween 替代手写动画
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

        /// <summary>
        /// 锁定按钮（例如玩家已经坐在别的地方了，其他空位就不能点了）
        /// </summary>
        public void SetInteractable(bool canInteract)
        {
            if (SitButton) SitButton.interactable = canInteract;
        }

        /// <summary>
        /// 显示玩家出的牌
        /// </summary>
        public void ShowPlayedCards(Card[] cards)
        {
            ClearPlayedCards();
            if (ActionBubble) ActionBubble.SetActive(false);

            if (cards == null || cards.Length == 0) return;

            if (PlayedCardsContainer == null)
            {
                Debug.LogError($"SeatView {SeatID}: PlayedCardsContainer is not assigned!");
                return;
            }
            if (CardViewPrefab == null)
            {
                Debug.LogError($"SeatView {SeatID}: CardViewPrefab is not assigned!");
                return;
            }

            // 使用 UniTask 启动异步方法
            AnimateShowCardsAsync(cards).Forget();
        }

        async UniTaskVoid AnimateShowCardsAsync(Card[] cards)
        {
            // 1. 创建临时动画层 (如果不存在)
            // 为了简单，我们直接在 PlayedCardsContainer 的父物体下创建一个临时的 RectTransform
            // 或者直接使用 Canvas 的根节点。这里为了坐标转换方便，使用 PlayedCardsContainer 的父节点。
            Transform animRoot = PlayedCardsContainer.parent; 
            
            // 2. 获取起点 (头像位置)
            Vector3 startWorldPos = PlayerInfoGroup != null ? PlayerInfoGroup.transform.position : transform.position;

            // 3. 预先生成占位符，让 LayoutGroup 计算目标位置
            List<GameObject> placeholders = new List<GameObject>();
            List<Vector3> targetPositions = new List<Vector3>();

            // 临时禁用 LayoutGroup 的自动更新，或者强制立即更新
            // 这里我们采用生成不可见占位符的方法
            foreach (var card in cards)
            {
                GameObject placeholder = Instantiate(CardViewPrefab, PlayedCardsContainer);
                // 设为透明或禁用 Image，只保留 RectTransform 占位
                var canvasGroup = placeholder.GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = placeholder.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 0; 
                
                placeholders.Add(placeholder);
            }

            // 强制刷新 Layout，获取正确的目标位置
            LayoutRebuilder.ForceRebuildLayoutImmediate(PlayedCardsContainer as RectTransform);

            // 记录每个占位符的世界坐标
            foreach (var p in placeholders)
            {
                targetPositions.Add(p.transform.position);
            }

            // 4. 生成真正的卡牌并做动画
            List<GameObject> realCards = new List<GameObject>();
            List<UniTask> animTasks = new List<UniTask>();

            for (int i = 0; i < cards.Length; i++)
            {
                var card = cards[i];
                Vector3 targetPos = targetPositions[i];

                // 在动画层生成卡牌
                GameObject go = Instantiate(CardViewPrefab, animRoot);
                go.transform.position = startWorldPos;
                go.transform.localScale = Vector3.zero; // 初始缩放为0

                CardView view = go.GetComponent<CardView>();
                if (view != null)
                {
                    view.Initialize(card);
                    var btn = view.GetComponent<Button>();
                    if (btn) Destroy(btn);
                    if (view.CardImage) view.CardImage.raycastTarget = false;
                }
                
                realCards.Add(go);

                // DOTween 动画序列
                var seq = DOTween.Sequence();
                seq.Append(go.transform.DOMove(targetPos, 0.3f).SetEase(Ease.OutBack)); // 飞入
                seq.Join(go.transform.DOScale(Vector3.one * 0.7f, 0.3f)); // 变大
                
                // 稍微错开每个牌的动画时间
                seq.SetDelay(i * 0.05f);
                
                animTasks.Add(seq.AsyncWaitForCompletion().AsUniTask());
            }

            // 5. 等待所有动画完成
            await UniTask.WhenAll(animTasks);

            // 6. 将卡牌归位到 PlayedCardsContainer
            for (int i = 0; i < realCards.Count; i++)
            {
                if (realCards[i] != null && placeholders[i] != null)
                {
                    realCards[i].transform.SetParent(PlayedCardsContainer);
                    // 重新设置顺序，确保覆盖占位符
                    realCards[i].transform.SetSiblingIndex(placeholders[i].transform.GetSiblingIndex());
                    // 确保位置和缩放正确 (LayoutGroup 会接管位置，我们只需重置缩放)
                    realCards[i].transform.localScale = Vector3.one * 0.7f;
                }
            }

            // 7. 销毁占位符
            foreach (var p in placeholders)
            {
                if (p != null) Destroy(p);
            }
        }

        /// <summary>
        /// 显示动作文字（如“不要”）
        /// </summary>
        public void ShowActionText(string text)
        {
            ClearPlayedCards();
            if (ActionBubble) ActionBubble.SetActive(true);
            if (ActionText) ActionText.text = text;
        }

        /// <summary>
        /// 清空出牌区
        /// </summary>
        public void ClearPlayedCards()
        {
            // 停止所有 DOTween 动画 (针对当前对象及其子对象)
            this.transform.DOKill(true);
            
            if (PlayedCardsContainer != null)
            {
                foreach (Transform child in PlayedCardsContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            // 也要清理可能还在动画层（父节点）的临时卡牌
            // 这比较难追踪，但由于我们使用了 await，通常 ClearPlayedCards 会在动画完成后或新一轮开始时调用
            // 如果动画还在播放就被 Clear，DOTween 的 Kill 会停止它们，但对象可能残留
            // 更好的做法是记录 realCards 列表并在 Clear 时销毁
            
            if (ActionBubble) ActionBubble.SetActive(false);
        }
    }
}