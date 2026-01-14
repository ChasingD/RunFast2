using Mirror;
using UnityEngine;
using RunFast2.Scripts.Model; // 引用 RoomSettings

namespace RunFast2.Scripts.Network
{
    public class RunFastNetworkManager : NetworkManager
    {
        // 1. 增加一个变量，用来“携带”房间设置
        public RoomSettings PendingRoomSettings;

        // 当服务器（主机）完成场景切换时，Mirror 会自动调用这个方法
        // public override void OnServerSceneChanged(string sceneName)
        // {
        //     base.OnServerSceneChanged(sceneName);
        //
        //     // 2. 判断是否进入了游戏场景
        //     // 确保你在 NetworkManager Inspector 的 Online Scene 填的是 "GameScene" (或你的场景名)
        //     if (sceneName.Contains("Game") || sceneName.Contains("Play")) 
        //     {
        //         // 3. 此时 PokerManager 已经加载完毕（Awake已执行），可以初始化了
        //         if (PokerManager.Instance != null && PendingRoomSettings != null)
        //         {
        //             Debug.Log($"[Server] 场景加载完毕，使用缓存配置初始化游戏...");
        //             PokerManager.Instance.InitializeGame(PendingRoomSettings);
        //             
        //             // (可选) 清空，防止下次误用
        //             PendingRoomSettings = null; 
        //         }
        //         else
        //         {
        //             Debug.LogError("PokerManager 缺失 或 没有房间设置数据！");
        //         }
        //     }
        // }
    }
}