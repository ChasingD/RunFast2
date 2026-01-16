using System;
using RunFast2.Scripts.Model;
using UnityEngine;
using UnityEngine.UI;

namespace RunFast2.Scripts.View
{
    public class ItemMenu : MonoBehaviour
    {
        [Header("Buttons")]
        public Button TomatoButton;
        public Button FlowerButton;
        public Button BucketButton;
        public Button BombButton;
        
        [Header("Close")]
        public Button BackgroundButton; // 点击背景关闭

        private int _targetSeatIndex;
        private Action<int, ItemType> _onItemClicked;

        public void Initialize(int targetSeatIndex, Action<int, ItemType> onItemClicked)
        {
            _targetSeatIndex = targetSeatIndex;
            _onItemClicked = onItemClicked;
        }

        private void Start()
        {
            if (TomatoButton) TomatoButton.onClick.AddListener(() => OnClick(ItemType.Tomato));
            if (FlowerButton) FlowerButton.onClick.AddListener(() => OnClick(ItemType.Flower));
            if (BucketButton) BucketButton.onClick.AddListener(() => OnClick(ItemType.Bucket));
            if (BombButton) BombButton.onClick.AddListener(() => OnClick(ItemType.Bomb));

            if (BackgroundButton) BackgroundButton.onClick.AddListener(Close);
        }

        private void OnClick(ItemType type)
        {
            _onItemClicked?.Invoke(_targetSeatIndex, type);
            Close();
        }

        private void Close()
        {
            Destroy(gameObject);
        }
    }
}