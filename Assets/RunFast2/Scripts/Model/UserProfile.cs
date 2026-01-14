using System;
using Postgrest.Attributes; // 修正引用
using Postgrest.Models;     // 修正引用

namespace RunFast2.Scripts.Model
{
    [Table("profiles")]
    public class UserProfile : BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("last_login_token")]
        public string LastLoginToken { get; set; }
        
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
    }
}