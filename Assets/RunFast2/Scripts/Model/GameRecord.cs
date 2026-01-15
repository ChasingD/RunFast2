using System;
using Postgrest.Attributes;
using Postgrest.Models;

namespace RunFast2.Scripts.Model
{
    [Table("game_records")]
    public class GameRecord : BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; }

        [Column("user_id")]
        public string UserId { get; set; }

        [Column("score_change")]
        public int ScoreChange { get; set; }

        [Column("is_winner")]
        public bool IsWinner { get; set; }

        [Column("is_robber")]
        public bool IsRobber { get; set; }

        [Column("is_rob_success")]
        public bool IsRobSuccess { get; set; }

        [Column("is_reverse_success")]
        public bool IsReverseSuccess { get; set; }

        [Column("bomb_count")]
        public int BombCount { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}