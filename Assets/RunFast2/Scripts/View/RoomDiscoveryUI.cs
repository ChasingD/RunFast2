using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.UI;

namespace RunFast2.Scripts.View
{
    public class RoomDiscoveryUI : MonoBehaviour
    {
        [Header("References")]
        public NetworkDiscovery networkDiscovery; // 拖入场景中的 NetworkDiscovery 组件
        public Transform ContentRoot;             // ScrollView 的 Content
        public GameObject RoomItemPrefab;         // RoomItemView 的预制体
        public Button RefreshButton;              // 刷新按钮
        public Button JoinButton;                 // 加入按钮
        public Button CloseButton;                // 关闭按钮

        // 缓存已发现的服务器
        private Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();
        private List<RoomItemView> currentItems = new List<RoomItemView>();
        
        private RoomItemView _selectedRoom;

        private void Start()
        {
            if (RefreshButton) RefreshButton.onClick.AddListener(StartSearch);
            if (JoinButton) JoinButton.onClick.AddListener(OnJoinClicked);
            if (CloseButton) CloseButton.onClick.AddListener(() => gameObject.SetActive(false));

            // 自动查找 NetworkDiscovery
            if (networkDiscovery == null)
            {
                networkDiscovery = FindObjectOfType<NetworkDiscovery>();
            }

            // 订阅发现事件
            if (networkDiscovery != null)
            {
                networkDiscovery.OnServerFound.AddListener(OnDiscoveredServer);
            }
            
            UpdateJoinButtonState();
        }

        private void OnEnable()
        {
            // 每次打开界面时自动搜索
            StartSearch();
        }

        public void StartSearch()
        {
            if (networkDiscovery == null) return;

            // 清空旧数据
            discoveredServers.Clear();
            ClearList();
            _selectedRoom = null;
            UpdateJoinButtonState();

            // 停止之前的搜索（如果有）并重新开始
            networkDiscovery.StopDiscovery();
            networkDiscovery.StartDiscovery();
            
            Debug.Log("正在搜索局域网房间...");
        }

        void OnDiscoveredServer(ServerResponse info)
        {
            // 如果已经包含该服务器，则忽略
            if (discoveredServers.ContainsKey(info.serverId)) return;

            discoveredServers[info.serverId] = info;
            AddRoomItem(info);
        }

        void AddRoomItem(ServerResponse info)
        {
            if (RoomItemPrefab == null || ContentRoot == null) return;

            GameObject go = Instantiate(RoomItemPrefab, ContentRoot);
            RoomItemView view = go.GetComponent<RoomItemView>();
            if (view != null)
            {
                view.Initialize(info, OnRoomSelected);
                currentItems.Add(view);
            }
        }

        void OnRoomSelected(RoomItemView selectedView)
        {
            // 更新选中状态
            _selectedRoom = selectedView;

            // 更新所有 Item 的视觉效果
            foreach (var item in currentItems)
            {
                item.SetSelected(item == selectedView);
            }
            
            UpdateJoinButtonState();
        }

        void OnJoinClicked()
        {
            if (_selectedRoom == null) return;

            // 停止搜索
            if (networkDiscovery != null) networkDiscovery.StopDiscovery();

            // 连接服务器
            NetworkManager.singleton.StartClient(_selectedRoom.Info.uri);
            
            // 关闭当前界面
            gameObject.SetActive(false);
        }

        void UpdateJoinButtonState()
        {
            if (JoinButton != null)
            {
                JoinButton.interactable = (_selectedRoom != null);
            }
        }

        void ClearList()
        {
            foreach (var item in currentItems)
            {
                Destroy(item.gameObject);
            }
            currentItems.Clear();
        }
    }
}