using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks; // 引用 UniTask

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

        [Header("Played Cards Area")]
        public Transform PlayedCardsContainer; // 出牌区域容器
        public GameObject CardViewPrefab;      // 牌的预制体
        public TextMeshProUGUI ActionText;     // 显示"不要"、"抢关"等文字

        [Header("Settings")]
        public int SeatID;                 // 0, 1, 2 (在Inspector里手动填好)

        // 定义一个委托，当点击入座时通知上层
        public System.Action<int> OnSitClicked;

        private void Start()
        {
            if (SitButton) SitButton.onClick.AddListener(() => OnSitClicked?.Invoke(SeatID));
            // 初始状态：空座位
            SetState_Empty();
            ClearPlayedCards();
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
            ClearPlayedCards();
        }

        /// <summary>
        /// 状态：有人入座
        /// </summary>
        public void SetState_Occupied(string playerName, bool isReady, bool isSelf, int cardCount)
        {
            if (SitButton) SitButton.gameObject.SetActive(false); // 隐藏入座按钮
            if (PlayerInfoGroup != null) PlayerInfoGroup.SetActive(true); // 显示玩家信息

            // 显示名字和剩余牌数
            string countStr = cardCount > 0 ? $" [{cardCount}]" : "";
            if (NameText) NameText.text = (isSelf ? $"<color=yellow>{playerName}</color>" : playerName) + countStr;
            
            if (ReadyIcon) ReadyIcon.gameObject.SetActive(isReady);
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
            if (ActionText) ActionText.gameObject.SetActive(false);

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
            foreach (var card in cards)
            {
                GameObject go = Instantiate(CardViewPrefab, PlayedCardsContainer);
                // 调整缩放，因为出牌区的牌通常比手牌小一点
                go.transform.localScale = Vector3.one * 0.6f; 
                
                // 确保位置归零 (如果 LayoutGroup 控制则会自动调整，但重置一下保险)
                go.transform.localPosition = Vector3.zero;

                CardView view = go.GetComponent<CardView>();
                if (view != null)
                {
                    view.Initialize(card);
                    // 出牌区的牌不需要交互，移除 Button 和 点击事件
                    var btn = view.GetComponent<Button>();
                    if (btn) Destroy(btn);
                    
                    // 如果不想让它阻挡射线（比如挡住后面的牌），可以把 Image 的 RaycastTarget 关掉
                    if (view.CardImage) view.CardImage.raycastTarget = false;
                }

                // 简单的弹跳动画
                go.transform.localScale = Vector3.zero;
                float timer = 0;
                while (timer < 0.2f)
                {
                    timer += Time.deltaTime;
                    if (go != null)
                        go.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 0.6f, timer / 0.2f);
                    await UniTask.Yield(); // 等待下一帧
                }
                if (go != null)
                    go.transform.localScale = Vector3.one * 0.6f;
            }
        }

        /// <summary>
        /// 显示动作文字（如“不要”）
        /// </summary>
        public void ShowActionText(string text)
        {
            ClearPlayedCards();
            if (ActionText)
            {
                ActionText.gameObject.SetActive(true);
                ActionText.text = text;
            }
        }

        /// <summary>
        /// 清空出牌区
        /// </summary>
        public void ClearPlayedCards()
        {
            // UniTask 不需要 StopAllCoroutines，但如果需要取消正在进行的任务，需要使用 CancellationToken
            // 这里简单处理，直接销毁对象即可，正在进行的动画如果对象被销毁了，判空即可
            if (PlayedCardsContainer != null)
            {
                foreach (Transform child in PlayedCardsContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            if (ActionText) ActionText.gameObject.SetActive(false);
        }
    }
}