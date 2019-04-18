﻿#region USING_DIRECTIVES

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion USING_DIRECTIVES

namespace Sol.Database.Models
{
    [Table("filters")]
    public class DatabaseFilter
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("DbGuildConfig")]
        [Column("gid")]
        public long GuildIdDb { get; set; }

        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("trigger"), Required, MaxLength(128)]
        public string Trigger { get; set; }

        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }
    }
}