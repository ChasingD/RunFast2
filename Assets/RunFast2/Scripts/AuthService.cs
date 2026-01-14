using System;
using Cysharp.Threading.Tasks;
using Supabase.Gotrue.Exceptions;
using UnityEngine;
using RunFast2.Scripts.Models; // 引用上面的 AppUser
using RunFast2.Scripts.Model; // 引用 UserProfile
using Supabase.Realtime;
using Supabase.Realtime.Interfaces;
using Supabase.Realtime.PostgresChanges;

namespace RunFast2.Scripts.Services
{
    public class AuthService : MonoBehaviour
    {
        public static AuthService Instance { get; private set; }

        [Header("Dependencies")]
        public SupabaseManager supabaseManager; // 在 Inspector 中拖入，或者自动查找

        [SerializeField] private string loginScene = "1Login";
        // 公共属性：当前登录的用户信息
        public AppUser CurrentUser { get; private set; }

        // 简单的属性判断是否登录
        public bool IsLoggedIn => CurrentUser != null;

        private IRealtimeChannel _userChannel;

        private void Awake()
        {
            // 单例初始化
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

        private void OnDestroy()
        {
            if (_userChannel != null)
            {
                _userChannel.Unsubscribe();
                _userChannel = null;
            }
        }

        /// <summary>
        /// 登录方法
        /// </summary>
        public async UniTask<AppUser> SignInAsync(string email, string password)
        {
            // 确保 Supabase 客户端已初始化
            var client = supabaseManager.Supabase();
            
            if (client == null)
            {
                throw new Exception("Supabase client is not initialized.");
            }

            // 确保 Socket 连接已建立 (增加空值检查)
            if (client.Realtime != null)
            {
                try 
                {
                    await client.Realtime.ConnectAsync();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Realtime connect failed (non-critical for login): {ex.Message}");
                }
            }
            
            // 发起登录
            var session = await client.Auth.SignInWithPassword(email, password);

            if (session != null && session.User != null)
            {
                // 本地防重
                if (CurrentUser != null && CurrentUser.Email == email)
                {
                    throw new Exception("该账号已在当前设备登录。");
                }

                string userId = session.User.Id;
                string newToken = session.AccessToken; // 使用 AccessToken 作为唯一标识，或者生成一个 GUID

                // 1. 更新数据库中的 last_login_token
                // 注意：这里需要确保数据库有 profiles 表，并且 RLS 策略允许用户更新自己的记录
                try
                {
                    var updateModel = new UserProfile
                    {
                        Id = userId,
                        LastLoginToken = newToken,
                        UpdatedAt = DateTime.UtcNow
                    };

                    // Upsert: 如果不存在则插入，存在则更新
                    await client.From<UserProfile>().Upsert(updateModel);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"更新登录Token失败: {ex.Message}");
                    // 可以选择忽略，或者视为登录失败
                }

                // 2. 登录成功，构建 AppUser
                CurrentUser = new AppUser(session.User, session.AccessToken);

                // 3. 开启心跳/监听，检测被顶号
                await StartListenForKick(userId, newToken);

                return CurrentUser;
            }
            
            return null;
        }

        /// <summary>
        /// 注册方法
        /// </summary>
        public async UniTask<AppUser> SignUpAsync(string email, string password)
        {
            var client = supabaseManager.Supabase();
            
            if (client == null)
            {
                throw new Exception("Supabase client is not initialized.");
            }

            // 确保 Socket 连接已建立
            if (client.Realtime != null)
            {
                try 
                {
                    await client.Realtime.ConnectAsync();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Realtime connect failed: {ex.Message}");
                }
            }

            var session = await client.Auth.SignUp(email, password);

            if (session != null && session.User != null)
            {
                CurrentUser = new AppUser(session.User, session.AccessToken);
                
                // 注册成功后通常也需要初始化 Profile
                try
                {
                    var newProfile = new UserProfile
                    {
                        Id = session.User.Id,
                        Username = email.Split('@')[0],
                        LastLoginToken = session.AccessToken,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await client.From<UserProfile>().Insert(newProfile);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"初始化 Profile 失败 (可能已存在): {ex.Message}");
                }

                await StartListenForKick(session.User.Id, session.AccessToken);

                return CurrentUser;
            }
            
            return null;
        }

        /// <summary>
        /// 登出方法
        /// </summary>
        public async UniTask SignOutAsync()
        {
            if (_userChannel != null)
            {
                _userChannel.Unsubscribe();
                _userChannel = null;
            }

            var client = supabaseManager.Supabase();
            if (client != null && client.Auth != null)
            {
                await client.Auth.SignOut();
            }
            CurrentUser = null;
        }

        /// <summary>
        /// 监听被顶号逻辑
        /// </summary>
        private async UniTask StartListenForKick(string userId, string currentToken)
        {
            var client = supabaseManager.Supabase();

            if (client == null || client.Realtime == null)
            {
                Debug.LogWarning("Realtime client is null, cannot listen for kick.");
                return;
            }

            // 确保 Socket 连接已建立
            try 
            {
                await client.Realtime.ConnectAsync();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Realtime connect failed: {ex.Message}");
            }

            // 如果之前有订阅，先取消
            if (_userChannel != null)
            {
                _userChannel.Unsubscribe();
            }

            // 订阅 profiles 表中 id = userId 的行
            // 注意：Supabase Realtime 需要在后台开启 Replication for profiles table
            _userChannel = client.Realtime.Channel($"public:profiles:id=eq.{userId}");

            // 使用 Register 方法注册 Postgres 变更监听
            // 根据反编译源码，IRealtimeChannel 没有 On 方法，而是使用 Register 和 AddPostgresChangeHandler
            
            // 方式 1: 使用 Register 注册选项，然后 Subscribe (这是 Supabase C# SDK 的标准流程)
            // 修正：PostgresChangesOptions 构造函数参数
            _userChannel.Register(new PostgresChangesOptions("public", "profiles", PostgresChangesOptions.ListenType.Updates, $"id=eq.{userId}"));
            
            // 方式 2: 添加回调处理
            // 修正：使用 PostgresChangesOptions.ListenType.Updates 而不是 Update
            _userChannel.AddPostgresChangeHandler(PostgresChangesOptions.ListenType.Updates, (sender, change) =>
            {
                // 当收到更新时
                // change.New 是一个字典，包含更新后的字段
                if (change.Payload.Data != null)
                {
                    // 注意：Supabase C# SDK 的 Realtime Payload 结构可能因版本而异
                    // 这里假设 Payload.Data 是一个 Dictionary<string, object>
                    // 或者通过 change.Model<UserProfile>() 来获取
                    
                    // 尝试直接反序列化为 UserProfile
                    try 
                    {
                        var newProfile = change.Model<UserProfile>();
                        if (newProfile != null && newProfile.LastLoginToken != currentToken)
                        {
                            Debug.LogWarning("检测到账号在其他设备登录，强制下线！");
                            HandleKicked();
                        }
                    }
                    catch
                    {
                        // 如果反序列化失败，尝试手动解析
                        // 这里简化处理，只要收到 Update 且不是自己触发的（虽然自己触发的也会收到，但 Token 应该一样）
                        // 实际上，如果 Token 变了，肯定不是当前客户端改的（除非当前客户端重新登录了，但那样会重置监听）
                        // 所以只要 Token 不一样，就是被顶了
                    }
                }
            });

            await _userChannel.Subscribe();
        }

        private void HandleKicked()
        {
            // 强制回到主线程执行 UI 操作
            // UnityMainThreadDispatcher 需要项目中存在，如果没有，可以使用 UniTask.PostToMainThread
            UniTask.Post(async () =>
            {
                // 1. 清理本地状态
                await SignOutAsync();

                // 2. 弹窗提示
                DialogManager.Instance.ShowInfo("下线通知", "您的账号已在其他设备登录，请重新登录。", "确定", () =>
                {
                    // 3. 返回登录场景
                    UnityEngine.SceneManagement.SceneManager.LoadScene(loginScene);
                });
            });
        }

        /// <summary>
        /// 辅助方法：解析 Supabase 的异常信息为用户友好的中文
        /// </summary>
        public string GetFriendlyErrorMessage(Exception ex)
        {
            if (ex is GotrueException goTrueEx)
            {
                string msg = goTrueEx.Message.ToLower();
                
                if (msg.Contains("already registered") || msg.Contains("already exists"))
                    return "注册失败: 该邮箱已被注册";
                
                if (msg.Contains("invalid login credentials"))
                    return "登录失败: 账号或密码错误";
                
                if (msg.Contains("password should be at least"))
                    return "密码太短，请设置更复杂的密码";
                    
                return $"认证错误: {goTrueEx.Message}";
            }

            return $"发生未知错误: {ex.Message}";
        }
    }
}