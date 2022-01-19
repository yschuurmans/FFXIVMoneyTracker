using System;
using System.Collections.Generic;

namespace FFXIVMoneyTracker.Models
{
    public class CharacterModel
    {
        public string Name { get; set; }
        public object World { get; set; }
        public long CurrentAmount { get; set; }
        public List<MoneyTransaction> Transactions { get; set; } = new List<MoneyTransaction>();
    }
}
