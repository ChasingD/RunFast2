using System.Collections.Generic;
using Ricimi;
using RunFast2.Scripts.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RunFast2.Scripts.View
{
    [RequireComponent(typeof(Popup))]
    public class RoundResultPanel : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI TitleText;
        public Transform ContentRoot;
        public GameObject ItemPrefab;
        public Button CloseButton;

        private Popup _popup;

        private void Awake()
        {
            _popup = GetComponent<Popup>();
            if (CloseButton) CloseButton.onClick.AddListener(Close);
        }

        // 统一命名为 Initialize，参数区分
        public void Initialize(RoundResult result)
        {
            if (TitleText) TitleText.text = $"第 {result.RoundIndex} 局结算";

            // Clear old items
            foreach (Transform child in ContentRoot)
            {
                Destroy(child.gameObject);
            }

            // Create new items
            foreach (var playerResult in result.PlayerResults)
            {
                GameObject go = Instantiate(ItemPrefab, ContentRoot);
                ResultItemView view = go.GetComponent<ResultItemView>();
                if (view != null)
                {
                    view.Initialize(playerResult);
                }
            }
        }

        public void Close()
        {
            if (_popup != null) _popup.Close();
        }
    }
}