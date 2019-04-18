﻿#region USING_DIRECTIVES

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion USING_DIRECTIVES

namespace Sol.Database.Models
{
    [Table("purchasable_items")]
    public class DatabasePurchasableItem
    {
        public DatabasePurchasableItem()
        {
            this.Purchases = new HashSet<DatabasePurchasedItem>();
        }

        [Key]
        [Column("id")]
        public int Id { get; set; }

        [ForeignKey("DbGuildConfig")]
        [Column("gid")]
        public long GuildIdDb { get; set; }

        [NotMapped]
        public ulong GuildId { get => (ulong)this.GuildIdDb; set => this.GuildIdDb = (long)value; }

        [Column("name"), Required, MaxLength(64)]
        public string Name { get; set; }

        [Column("price"), Required]
        public long Price { get; set; }

        public virtual DatabaseGuildConfig DbGuildConfig { get; set; }
        public virtual ICollection<DatabasePurchasedItem> Purchases { get; set; }
    }
}