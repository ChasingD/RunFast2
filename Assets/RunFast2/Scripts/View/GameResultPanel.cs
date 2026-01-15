using System.Collections.Generic;
using Ricimi;
using RunFast2.Scripts.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RunFast2.Scripts.View
{
    [RequireComponent(typeof(Popup))]
    public class GameResultPanel : MonoBehaviour
    {
        [Header("UI References")]
        public Transform ContentRoot;
        public GameObject ItemPrefab;
        public Button BackToLobbyButton;

        private Popup _popup;
        public System.Action OnBackToLobbyClicked;

        private void Awake()
        {
            _popup = GetComponent<Popup>();
            if (BackToLobbyButton) BackToLobbyButton.onClick.AddListener(() => OnBackToLobbyClicked?.Invoke());
        }

        public void Initialize(GameTotalResult result)
        {
            // Clear old items
            foreach (Transform child in ContentRoot)
            {
                Destroy(child.gameObject);
            }

            // Create new items
            foreach (var playerStats in result.PlayerStats)
            {
                GameObject go = Instantiate(ItemPrefab, ContentRoot);
                ResultItemView view = go.GetComponent<ResultItemView>();
                if (view != null)
                {
                    view.Initialize(playerStats);
                }
            }
        }

        public void Close()
        {
            if (_popup != null) _popup.Close();
        }
    }
}