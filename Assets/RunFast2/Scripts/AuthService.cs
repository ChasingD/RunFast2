using System;
using Cysharp.Threading.Tasks;
using Supabase.Gotrue.Exceptions;
using UnityEngine;
using RunFast2.Scripts.Models; // 引用上面的 AppUser

namespace RunFast2.Scripts.Services
{
    public class AuthService : MonoBehaviour
    {
        public static AuthService Instance { get; private set; }

        [Header("Dependencies")]
        public SupabaseManager supabaseManager; // 在 Inspector 中拖入，或者自动查找

        // 公共属性：当前登录的用户信息
        public AppUser CurrentUser { get; private set; }

        // 简单的属性判断是否登录
        public bool IsLoggedIn => CurrentUser != null;

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

        /// <summary>
        /// 登录方法
        /// </summary>
        public async UniTask<AppUser> SignInAsync(string email, string password)
        {
            // 确保 Supabase 客户端已初始化
            var client = supabaseManager.Supabase();
            
            // 发起登录
            var session = await client.Auth.SignInWithPassword(email, password);

            if (session != null && session.User != null)
            {
                // 登录成功，构建 AppUser
                CurrentUser = new AppUser(session.User, session.AccessToken);
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

            var session = await client.Auth.SignUp(email, password);

            if (session != null && session.User != null)
            {
                // 注意：如果需要邮箱验证，这里 session 可能受限，视后台配置而定
                // 暂时我们也更新 CurrentUser，或者让用户重新登录
                CurrentUser = new AppUser(session.User, session.AccessToken);
                return CurrentUser;
            }
            
            return null;
        }

        /// <summary>
        /// 登出方法
        /// </summary>
        public async UniTask SignOutAsync()
        {
            var client = supabaseManager.Supabase();
            if (client.Auth != null)
            {
                await client.Auth.SignOut();
            }
            CurrentUser = null;
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