using System;
using UIWidgets;
using UnityEngine;

// 用于方便处理数组

namespace RunFast2.Scripts
{
    public class DialogManager : MonoBehaviour
    {
        // 1. 单例实例
        public static DialogManager Instance { get; private set; }

        [Header("Template")]
        [SerializeField]
        [Tooltip("请将 Dialog 的 Prefab 拖拽到这里，而不是场景里的对象")]
        protected Dialog DialogSampleTemplate;

        private void Awake()
        {
            // 2. 单例初始化与 DontDestroyOnLoad
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 显示一个包含自定义按钮的通用对话框
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="message">消息内容</param>
        /// <param name="buttons">按钮数组 (可选，默认为 Close)</param>
        /// <param name="focusButton">默认聚焦的按钮文本 (可选)</param>
        /// <param name="targetCanvas">指定显示的 Canvas (可选，自动查找)</param>
        public void ShowDialog(string title, string message, DialogButton[] buttons = null, string focusButton = null, Canvas targetCanvas = null)
        {
            // 如果没有传入按钮，默认创建一个 Close 按钮
            if (buttons == null || buttons.Length == 0)
            {
                buttons = new DialogButton[] { new DialogButton("Close") };
            }

            // 如果没有指定聚焦按钮，默认聚焦第一个
            if (string.IsNullOrEmpty(focusButton))
            {
                focusButton = buttons[0].Label;
            }

            // 3. 处理 Canvas 查找逻辑
            // 因为 Manager 是 DontDestroy 的，它自己的 transform 可能不在当前活动场景的 Canvas 下
            // 如果未指定 canvas，尝试找场景里活动的 Canvas
            if (targetCanvas == null)
            {
                // 优先尝试找 Main Canvas，找不到则找任意一个
                targetCanvas = FindObjectOfType<Canvas>();
                
                if (targetCanvas == null)
                {
                    Debug.LogError("DialogManager: 场景中找不到任何 Canvas，无法显示对话框！");
                    return;
                }
            }

            // 克隆模板
            var dialog = DialogSampleTemplate.Clone();

            // 显示
            dialog.Show(
                title: title,
                message: message,
                buttons: buttons,
                focusButton: focusButton,
                canvas: targetCanvas
            );
        }

        /// <summary>
        /// 快速显示一个只有“确定”或“关闭”按钮的信息弹窗
        /// </summary>
        public void ShowInfo(string title, string message, string closeBtnText = "关闭", Action onClose = null)
        {
            var btn = new DialogButton(closeBtnText, () => 
            {
                onClose?.Invoke();
                return true; // 返回 true 关闭对话框
            });

            ShowDialog(title, message, new DialogButton[] { btn }, closeBtnText);
        }
        
        /// <summary>
        /// 快速显示一个确认/取消对话框
        /// </summary>
        public void ShowConfirm(string title, string message, Action onConfirm, Action onCancel = null)
        {
            var btnConfirm = new DialogButton("Yes", () => 
            {
                onConfirm?.Invoke();
                return true; 
            });

            var btnCancel = new DialogButton("No", () => 
            {
                onCancel?.Invoke();
                return true; 
            });

            ShowDialog(title, message, new DialogButton[] { btnCancel, btnConfirm }, "Yes");
        }
    }
}