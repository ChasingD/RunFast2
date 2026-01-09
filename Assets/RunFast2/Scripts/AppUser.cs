using System;

namespace RunFast2.Scripts.Models
{
    [Serializable]
    public class AppUser
    {
        public string Id;
        public string Email;
        public string Token;
        // 你可以在这里扩展更多字段，比如 Nickname, AvatarUrl 等
        
        // 构造函数：从 Supabase User 转换
        public AppUser(Supabase.Gotrue.User supabaseUser, string sessionToken)
        {
            if (supabaseUser != null)
            {
                Id = supabaseUser.Id;
                Email = supabaseUser.Email;
                Token = sessionToken;
            }
        }
    }
}