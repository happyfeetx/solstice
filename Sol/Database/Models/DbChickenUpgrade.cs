﻿#region USING_DIRECTIVES

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Sol.Modules.Chickens.Common;

#endregion USING_DIRECTIVES

namespace Sol.Database.Models
{
    [Table("chicken_upgrades")]
    public class DatabaseChickenUpgrade
    {
        public DatabaseChickenUpgrade()
        {
            this.BoughtUpgrades = new HashSet<DatabaseChickenBoughtUpgrade>();
        }

        [Key, Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Column("name"), Required, MaxLength(32)]
        public string Name { get; set; }

        [Column("cost")]
        public long Cost { get; set; }

        [Column("stat")]
        public ChickenStatUpgrade UpgradesStat { get; set; }

        [Column("mod")]
        public int Modifier { get; set; }

        public virtual ICollection<DatabaseChickenBoughtUpgrade> BoughtUpgrades { get; set; }
    }
}