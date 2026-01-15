using System.Collections.Generic;
using Ricimi;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RunFast2.Scripts.Model;

namespace RunFast2.Scripts.View
{
    [RequireComponent(typeof(Popup))]
    public class RoundResultPopup : MonoBehaviour
    {
        [Header("UI References")] public TextMeshProUGUI RoundTitleText;
        public Transform ContentRoot; // 用于放置 ResultItemView
        public GameObject ResultItemPrefab; // ResultItemView 的预制体
        public Button CloseButton;

        private Popup _popup;

        private void Awake()
        {
            _popup = GetComponent<Popup>();
        }

        private void Start()
        {
            if (CloseButton) CloseButton.onClick.AddListener(HandleCloseClick);
        }

        public void Initialize(RoundResult result, Dictionary<int, string> playerNames)
        {
            if (RoundTitleText) RoundTitleText.text = $"第 {result.RoundIndex} 局结算";

            // 清理旧条目
            foreach (Transform child in ContentRoot)
            {
                Destroy(child.gameObject);
            }

            // 添加新条目
            foreach (var playerResult in result.PlayerResults)
            {
                GameObject go = Instantiate(ResultItemPrefab, ContentRoot);
                ResultItemView view = go.GetComponent<ResultItemView>();
                if (view != null)
                {
                    view.Initialize(playerResult);
                }
            }
        }

        private void HandleCloseClick()
        {
            if (_popup != null) _popup.Close();
        }
    }
}