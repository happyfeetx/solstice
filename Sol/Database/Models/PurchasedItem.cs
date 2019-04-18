﻿#region USING_DIRECTIVES

using System.ComponentModel.DataAnnotations.Schema;

#endregion USING_DIRECTIVES

namespace Sol.Database.Models
{
    [Table("purchased_items")]
    public class DatabasePurchasedItem
    {
        [ForeignKey("DbPurchasableItem")]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ItemId { get; set; }

        [Column("uid")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long UserIdDb { get; set; }

        [NotMapped]
        public ulong UserId { get => (ulong)this.UserIdDb; set => this.UserIdDb = (long)value; }

        public virtual DatabasePurchasableItem DbPurchasableItem { get; set; }
    }
}