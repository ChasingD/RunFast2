using Mirror;

namespace RunFast2.Scripts.Network
{
    public class RunFastNetworkManager : NetworkManager
    {
        // 当服务器切换场景完成后调用（比如从 Lobby -> Game）
        public override void OnServerSceneChanged(string sceneName)
        {
            base.OnServerSceneChanged(sceneName);
        
            // 如果是游戏场景，可以在这里初始化一些环境数据
            if (sceneName == "GameScene")
            {
                // 例如：生成发牌器（如果它不是场景自带的话）
            }
        }

        // 当客户端断开连接时
        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            // 可以在这里处理断线重连或返回 Lobby 的逻辑
        }
    }
}