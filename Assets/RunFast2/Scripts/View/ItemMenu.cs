using System;
using RunFast2.Scripts.Model;
using RunFast2.Scripts.Network;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

namespace RunFast2.Scripts.View
{
    public class ItemMenu : MonoBehaviour
    {
        [Header("Buttons")]
        public Button TomatoButton;
        public Button FlowerButton;
        public Button BucketButton;
        public Button BombButton;
        public Button UpsideDownButton;
        public Button EarthquakeButton;
        
        [Header("Close")]
        public Button BackgroundButton; // 点击背景关闭

        private int _targetSeatIndex;

        public void Initialize(int targetSeatIndex)
        {
            _targetSeatIndex = targetSeatIndex;
        }

        private void Start()
        {
            if (TomatoButton) TomatoButton.onClick.AddListener(() => OnClick(ItemType.Tomato));
            if (FlowerButton) FlowerButton.onClick.AddListener(() => OnClick(ItemType.Flower));
            if (BucketButton) BucketButton.onClick.AddListener(() => OnClick(ItemType.Bucket));
            if (BombButton) BombButton.onClick.AddListener(() => OnClick(ItemType.Bomb));
            if (UpsideDownButton) UpsideDownButton.onClick.AddListener(() => OnClick(ItemType.UpsideDown));
            if (EarthquakeButton) EarthquakeButton.onClick.AddListener(() => OnClick(ItemType.Earthquake));

            if (BackgroundButton) BackgroundButton.onClick.AddListener(Close);
        }

        private void OnClick(ItemType type)
        {
            var localPlayer = NetworkClient.localPlayer?.GetComponent<CardPlayer>();
            if (localPlayer)
            {
                localPlayer.CmdUseItem(_targetSeatIndex, (int)type);
            }
            Close();
        }

        private void Close()
        {
            Destroy(gameObject);
        }
    }
}