using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.UI;

namespace RunFast2.Scripts.View
{
    public class RoomDiscoveryUI : MonoBehaviour
    {
        // 内部类，用于存储服务器信息和最后发现时间
        private class DiscoveredServerInfo
        {
            public ServerResponse Response;
            public float LastSeenTime;
            public RoomItemView UiItem;
        }

        [Header("References")]
        public NetworkDiscovery networkDiscovery; // 拖入场景中的 NetworkDiscovery 组件
        public Transform ContentRoot;             // ScrollView 的 Content
        public GameObject RoomItemPrefab;         // RoomItemView 的预制体
        public Button RefreshButton;              // 刷新按钮
        public Button JoinButton;                 // 加入按钮
        // public Button CloseButton;                // 关闭按钮

        [Header("Settings")]
        public float ServerTimeout = 5f; // 超过5秒未收到广播则认为服务器已消失

        // 缓存已发现的服务器
        private readonly Dictionary<long, DiscoveredServerInfo> _discoveredServers = new Dictionary<long, DiscoveredServerInfo>();
        private RoomItemView _selectedRoom;

        private void Start()
        {
            if (RefreshButton) RefreshButton.onClick.AddListener(StartSearch);
            if (JoinButton) JoinButton.onClick.AddListener(OnJoinClicked);
            // if (CloseButton) CloseButton.onClick.AddListener(() => gameObject.SetActive(false));

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

            StartSearch();
        }

        private void OnEnable()
        {
            // 每次打开界面时自动搜索
            StartSearch();
        }

        private void Update()
        {
            // 定期检查超时的服务器
            CheckForTimedOutServers();
        }

        public void StartSearch()
        {
            if (networkDiscovery == null) return;

            // 清空旧数据
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
            // 如果已经包含该服务器，则只更新时间戳
            if (_discoveredServers.ContainsKey(info.serverId))
            {
                _discoveredServers[info.serverId].LastSeenTime = Time.time;
            }
            else // 否则，这是一个新服务器
            {
                var newServerInfo = new DiscoveredServerInfo
                {
                    Response = info,
                    LastSeenTime = Time.time,
                    UiItem = CreateRoomItem(info)
                };
                _discoveredServers[info.serverId] = newServerInfo;
            }
        }

        RoomItemView CreateRoomItem(ServerResponse info)
        {
            if (RoomItemPrefab == null || ContentRoot == null) return null;

            GameObject go = Instantiate(RoomItemPrefab, ContentRoot);
            go.SetActive(true);
            RoomItemView view = go.GetComponent<RoomItemView>();
            if (view != null)
            {
                view.Initialize(info, OnRoomSelected);
            }
            return view;
        }

        void CheckForTimedOutServers()
        {
            // 使用 ToList() 创建一个副本进行遍历，因为我们可能会在循环中修改字典
            foreach (var server in _discoveredServers.Values.ToList())
            {
                if (Time.time - server.LastSeenTime > ServerTimeout)
                {
                    // 服务器超时，移除
                    RemoveServer(server.Response.serverId);
                }
            }
        }

        void RemoveServer(long serverId)
        {
            if (_discoveredServers.TryGetValue(serverId, out var serverInfo))
            {
                // 如果被移除的房间是当前选中的，则取消选中
                if (_selectedRoom != null && _selectedRoom == serverInfo.UiItem)
                {
                    _selectedRoom = null;
                    UpdateJoinButtonState();
                }

                // 销毁 UI 对象
                if (serverInfo.UiItem != null)
                {
                    Destroy(serverInfo.UiItem.gameObject);
                }

                // 从字典中移除
                _discoveredServers.Remove(serverId);
            }
        }

        void OnRoomSelected(RoomItemView selectedView)
        {
            // 更新选中状态
            _selectedRoom = selectedView;

            // 更新所有 Item 的视觉效果
            foreach (var item in _discoveredServers.Values)
            {
                if (item.UiItem != null)
                {
                    item.UiItem.SetSelected(item.UiItem == selectedView);
                }
            }
            
            UpdateJoinButtonState();
        }

        void OnJoinClicked()
        {
            if (_selectedRoom == null) return;

            // 再次检查房间是否有效 (防止在点击瞬间房间刚好超时被移除)
            if (!_discoveredServers.ContainsKey(_selectedRoom.Info.serverId))
            {
                DialogManager.Instance.ShowInfo("提示", "该房间已不存在或已关闭。");
                // 刷新列表
                StartSearch();
                return;
            }

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
            foreach (var server in _discoveredServers.Values)
            {
                if (server.UiItem != null)
                {
                    Destroy(server.UiItem.gameObject);
                }
            }
            _discoveredServers.Clear();
        }
    }
}